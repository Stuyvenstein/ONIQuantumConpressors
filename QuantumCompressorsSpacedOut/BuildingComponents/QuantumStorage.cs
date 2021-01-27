using KSerialization;
using QuantumCompressors.BuildingConfigs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUNING;
using UnityEngine;

namespace QuantumCompressors.BuildingComponents
{
    [SerializationConfig(MemberSerialization.OptIn)]
    [AddComponentMenu("KMonoBehaviour/scripts/"+nameof(QuantumStorage))]
    public class QuantumStorage:KMonoBehaviour, ISaveLoadable
    {
        public const float QuantumStorageMultiplier = 60f;
        public const float QuantumSolidStorageMultiplier = 30f;
        public const float QuantumStoragePowerConsume = 2000f;

        public static float[] IntakeCost = new float[]
        {
            200f,
            50f,
        };
        public static string[] IntakeMats = new string[]
    {
            MATERIALS.REFINED_METALS[0],
            SimHashes.Polypropylene.ToString()
    };
        public static float[] OutletCost = new float[]
        {
            300f,
            75f,
        };
        public static string[] OutletMats = new string[]
    {
            MATERIALS.REFINED_METALS[0],
            SimHashes.Polypropylene.ToString()
    };
        public static float[] DualPortCost = new float[]
        {
            450f,
            110f,
        };
        public static string[] DualPortMats = new string[]
    {
            MATERIALS.REFINED_METALS[0],
            SimHashes.Polypropylene.ToString()
    };

        public static float[] CompressorCost = new float[]
        {
            1200f,
            200f,
            400f
        };
        public static string[] CompressorMats = new string[]
    {
            SimHashes.Steel.ToString(),
            SimHashes.Tungsten.ToString(),
            MATERIALS.TRANSPARENTS[0]
    };

        [SerializeField]
        public ConduitType conduitType;
        [MyCmpReq]
        public Storage storage;
        [MyCmpReq]
        public Operational operational;
        private QuantumStorageItem itemConfig;
        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            
        }
        protected override void OnCleanUp()
        {
            BuildingComplete com = GetComponent<BuildingComplete>();
            QuantumStorageSingleton quantumStorage = QuantumStorageSingleton.Get();
            quantumStorage.StorageItems.Remove(itemConfig);
            base.OnCleanUp();
        }
        protected override void OnSpawn()
        {
            base.OnSpawn();
            BuildingComplete com = GetComponent<BuildingComplete>();
            itemConfig = new QuantumStorageItem
            {
                worldId = com.GetMyWorldId(),
                conduitType = conduitType,
                operational = operational,
                storage = storage
            };
            QuantumStorageSingleton quantumStorage = QuantumStorageSingleton.Get();
            quantumStorage.StorageItems.Add(itemConfig);
            //name = "QuantumStorage" + Enum.GetName(typeof(ConduitType), conduitType)+ com.GetMyWorldId().ToString();
        }
    }
}
