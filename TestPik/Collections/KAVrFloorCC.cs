using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace TestPik.Collections
{
    /// <summary>
    /// Этаж в секции Level
    /// </summary>
    public class KAVrFloorCC
    {
        /// <summary>
        /// Идентификатор секции
        /// </summary>
        public string SectionNameID { get; set; }
        /// <summary>
        /// Имя этажа
        /// </summary>
        public string LevelName { get; set; }
        /// <summary>
        /// Квартиры на этаже
        /// </summary>
        public List<KAVrApartmentCC> lstApartments { get; set; }
        /// <summary>
        /// ID уровня 
        /// </summary>
        public ElementId LevelID { get; set; }
    }
}
