using Autodesk.Revit.DB;

namespace TestPik.Collections
{
    /// <summary>
    /// ROM_Подзона
    /// </summary>
    public class KAVrRoomCC
    {
        /// <summary>
        /// ID комнаты
        /// </summary>
        public ElementId RoomID { get; set; }
        /// <summary>
        /// Имя помещения
        /// </summary>
        public string NameRoom { get; set; }
    }
}