using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TestPik.Collections;

namespace TestPik
{
    class GetRooms
    {
        // принцип работы:
        // 1) записываем всё в коллекции Блок->Этаж->Квартиры->Комнаты отсеяв не квартиры
        // 2) создаём коллекцию комнат которые надо перекрасить
        // 3) в транзакции меняем параметр на .Полутон ищем и меняем параметры через Look т.к. многоязычность не требуется
        public static List<KAVrSection> lstSection = new List<KAVrSection>();
        /// <summary>
        /// Изменение цвета у однотипных квартир расположенных рядом
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        internal static void GetRoomsPIK(Document document)
        {
            List<Room> lstRet = new List<Room>();
            IEnumerable<Room> col = new FilteredElementCollector(document)
                            .WhereElementIsNotElementType()
                            .OfClass(typeof(SpatialElement))
                            .Where(e => e.GetType() == typeof(Room)).Cast<Room>();

            if (col != null)
            {
                // 1) пункт
                // если комнаты есть то записываем всё в коллекцию
                foreach (var item in col)
                {
                    // определяем блок у квартиры
                    var _BS_block = item.LookupParameter("BS_Блок").AsString();

                    // Если секции нет то создаём
                    int index = lstSection.IndexOf(lstSection.Where(x => x.SectionName == _BS_block).FirstOrDefault());
                    if (index < 0)
                    {
                        lstSection.Add(new KAVrSection()
                        {
                            SectionName = _BS_block,
                            lstFloors = new List<KAVrFloorCC>()
                        });
                        // ПРисваиваем последний индекс вновь созданному блоку либо оставоляем старый
                        index = lstSection.Count - 1;
                    }

                    // Определяем этаж у квартиры
                    var _Level1 = item.LookupParameter("Уровень").AsValueString();
                    ElementId _levelId = item.LevelId;
                    var level_1 = item.Level;
                    // Ищем этаж в секции, если нет то создаём
                    int indexF = lstSection[index].lstFloors.IndexOf(lstSection[index].lstFloors.Where(x =>
                    x.SectionNameID == _BS_block     // в секции
                    && x.LevelName == _Level1        // этаж 
                    ).FirstOrDefault());
                    if (indexF < 0)
                    {
                        lstSection[index].lstFloors.Add(new KAVrFloorCC()
                        {
                            LevelName = _Level1,
                            LevelID = _levelId,
                            SectionNameID = _BS_block,
                            lstApartments = new List<KAVrApartmentCC>()
                        });
                        // ПРисваиваем последний индекс вновь созданному этажу либо оставоляем старый
                        indexF = lstSection[index].lstFloors.Count - 1;
                    }

                    // Создаём квартиру и добавляем комнаты
                    // Определяем Количество комнат
                    int RoomValue = GetRoomValue(item);
                    var Room_zona = item.LookupParameter("ROM_Зона").AsString();
                    var Room_PodZona_ID = item.LookupParameter("ROM_Расчетная_подзона_ID").AsString();
                    if (Room_zona.Contains("Квартира"))
                    {
                        // номер квартиры
                        int _numberAp = Convert.ToInt32(Room_zona.Split(' ')[1]);
                        // Поиск квартиры в блоке на этаже и по номеру есть или нет
                        int indexA = lstSection[index].lstFloors[indexF].lstApartments.IndexOf(lstSection[index].lstFloors[indexF].lstApartments.Where(x =>
                            x.SectionNameID == _BS_block            // блок
                            && x.LevelNameID == _Level1             // этаж    
                            && x.ApartmentNumber == _numberAp       // номер квартиры на этаже (уникальный должен быть)
                            && x.ValueRoom == RoomValue             // количество комнат (на всякий случай)
                                ).FirstOrDefault());
                        if (indexA < 0) // например двушки нет то создаём новую двушку
                        {
                            lstSection[index].lstFloors[indexF].lstApartments.Add(new KAVrApartmentCC()
                            {
                                SectionNameID = _BS_block,          // блок
                                LevelNameID = _Level1,              // этаж 
                                ApartmentNumber = _numberAp,        // номер квартиры
                                ValueRoom = RoomValue,              // количество комнат
                                lstRooms = new List<KAVrRoomCC>()
                                {
                                    new KAVrRoomCC()
                                    {
                                        RoomID = item.Id,
                                        NameRoom = Room_PodZona_ID
                                    }
                                }
                            });
                            indexA = lstSection[index].lstFloors[indexF].lstApartments.Count - 1;
                        }
                        else // если такая квартира уже есть то добавляем комнату
                        {
                            lstSection[index].lstFloors[indexF].lstApartments[indexA].lstRooms.Add(new KAVrRoomCC()
                            {
                                RoomID = item.Id,
                                NameRoom = Room_PodZona_ID
                            });
                        }
                    }
                }
                // 2) пункт
                // коллекция помещений требующих изменения цвета на полутон
                List<KAVrRoomCC> listRoomsDouble = new List<KAVrRoomCC>();
                // проходим по коллекции блоков зданий для поиска сдвоенных квартир
                if (lstSection != null && lstSection.Count > 0)
                {
                    for (int i = 0; i < lstSection.Count; i++)                  // проходим по секциям
                    {
                        for (int j = 0; j < lstSection[i].lstFloors.Count; j++) // проходим по этажам
                        {
                            // Сортируем квартиры по номерам группируем квартиры по количеству комнат и отсеиваем одиночные квартиры на этаже
                            var apGroup = lstSection[i].lstFloors[j].lstApartments.OrderBy(x => x.ValueRoom).OrderBy(x => x.ApartmentNumber).GroupBy(x => x.ValueRoom).Where(x => x.LongCount() > 1).Select(g => new
                            {
                                lstR = g,
                            }).ToList();

                            // пройтись по коллекции и изменить полутон 
                            if (apGroup != null && apGroup.Count() > 0)
                            {
                                // проходим по коллекции
                                foreach (var item in apGroup)
                                {
                                    int _last = -1;// маркер совпадает???
                                    foreach (var itemR in item.lstR)
                                    {
                                        if (itemR.ApartmentNumber == _last + 1) // если следующий номер следующий за предыдущим то добавляем ег ов коллекцию
                                        {
                                            foreach (var itRooms in itemR.lstRooms)
                                            {
                                                listRoomsDouble.Add(new KAVrRoomCC() { RoomID = itRooms.RoomID, NameRoom = itRooms.NameRoom }); // записали колмнаты для изменения на полутона
                                            }
                                            _last = itemR.ApartmentNumber + 1; // пропускаем следующий номер, если квартир больше 3
                                        }
                                        else
                                            _last = itemR.ApartmentNumber;
                                    }
                                }
                            }
                        }
                    }
                }
                // 3) Пункт
                // назначение параметра по заджанию
                if (listRoomsDouble != null && listRoomsDouble.Count > 0)
                {
                    try
                    {
                        using (Transaction tran = new Transaction(document, "Set Parameter"))
                        {
                            tran.Start();
                            for (int i = 0; i < listRoomsDouble.Count; i++)
                            {
                                // элемент комнаты
                                Element e = document.GetElement(listRoomsDouble[i].RoomID) as Element;
                                // параметр у элемента который надо поменять
                                Parameter parameter = e.LookupParameter("ROM_Подзона_Index");
                                // применяем параметр
                                parameter.Set(listRoomsDouble[i].NameRoom + ".Полутон");
                            }
                            tran.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Ошибка транзакции");
                        Debug.WriteLine(ex.ToString());
                    }
                }
            }
            //return lstRet;
        }
        /// <summary>
        /// Получение количества квартир
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static int GetRoomValue(Room item)
        {
            var r = item.LookupParameter("ROM_Подзона").AsString();
            if (r.Contains("Од"))
                return 1;
            else if (r.Contains("Дв"))
                return 2;
            else if (r.Contains("Тр"))
                return 3;
            else if (r.Contains("Че"))
                return 4;
            else
                return 1;
        }
    }
}
