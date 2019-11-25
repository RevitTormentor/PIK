using System.Collections.Generic;

namespace TestPik.Collections
{
    /// <summary>
    /// Блок зданий BS_Блок
    /// </summary>
    public class KAVrSection
    {
        /// <summary>
        /// Имя секции / блока
        /// </summary>
        public string SectionName { get; set; }
        /// <summary>
        /// Этажи в секции
        /// </summary>
        public List<KAVrFloorCC> lstFloors { get; set; }
    }
}
