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
            2000f,
            100f,
            400f
        };
        public static string[] CompressorMats = new string[]
    {
            MATERIALS.REFINED_METALS[0],
            SimHashes.SuperInsulator.ToString(),
            MATERIALS.TRANSPARENTS[0]
    };

        [SerializeField]
        public ConduitType conduitType;
        [MyCmpReq]
        public Storage storage;
        [MyCmpReq]
        public Operational operational;
        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            
        }
        protected override void OnSpawn()
        {
            base.OnSpawn();
            name = "QuantumStorage" + Enum.GetName(typeof(ConduitType), conduitType);
        }
    }
}
