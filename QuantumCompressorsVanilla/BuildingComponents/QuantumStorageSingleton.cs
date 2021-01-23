using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantumCompressors.BuildingComponents
{
    public class QuantumStorageSingleton
    {
        private static QuantumStorageSingleton instance;
        private QuantumStorageSingleton() { }
        public List<QuantumStorageItem> StorageItems = new List<QuantumStorageItem>();
        public BuildingDef gasCompBuildDef, liquidCompBuildDef, solidCompBuildDef;
        public static QuantumStorageSingleton Get()
        {
            if (instance == null) instance = new QuantumStorageSingleton();
            return instance;
        }

    }

    public class QuantumStorageItem
    {
        public Storage storage { get; set; }
        //public int worldId { get; set; }
        public Operational operational { get; set; }
        public ConduitType conduitType { get; set; }
    }

}
