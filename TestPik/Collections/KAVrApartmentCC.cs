using System.Collections.Generic;

namespace TestPik.Collections
{
    /// <summary>
    /// Квартира ROM_Зона
    /// </summary>
    public class KAVrApartmentCC
    {
        /// <summary>
        /// ID квартиры
        /// </summary>
        public int ApartmentNumber { get; set; }
        /// <summary>
        /// количество комнат
        /// </summary>
        public int ValueRoom { get; set; }
        /// <summary>
        /// секция блок
        /// </summary>        
        public string SectionNameID { get; set; }
        /// <summary>
        /// этаж
        /// </summary>        
        public string LevelNameID { get; set; }

        /// <summary>
        /// список комнат
        /// </summary>        
        public List<KAVrRoomCC> lstRooms { get; set; }
    }
}