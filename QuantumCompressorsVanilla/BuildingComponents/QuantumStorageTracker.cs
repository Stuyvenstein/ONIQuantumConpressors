using KSerialization;
using QuantumCompressors.BuildingConfigs.Gas;
using QuantumCompressors.BuildingConfigs.Liquid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QuantumCompressors.BuildingComponents
{
    [SerializationConfig(MemberSerialization.OptIn)]
    [AddComponentMenu("KMonoBehaviour/scripts/" + nameof(QuantumStorageTracker))]
    public class QuantumStorageTracker:KMonoBehaviour
    {
        [SerializeField]
        public ConduitType conduitType;
        
        protected override void OnSpawn()
        {
            base.OnSpawn();
            QuantumStorageSingleton storageSingleton = QuantumStorageSingleton.Get();
            PlayerController.Instance.ToolDeactivated(BuildTool.Instance);
            switch (conduitType)
            {
                case ConduitType.Gas:
                    if (storageSingleton.gasCompBuildDef == null)
                    {
                        storageSingleton.gasCompBuildDef = Assets.GetBuildingDef(GasQuantumCompressorConfig.ID);
                    }
                    Assets.BuildingDefs.Remove(storageSingleton.gasCompBuildDef);
                    break;
                case ConduitType.Liquid:
                    if (storageSingleton.liquidCompBuildDef == null)
                    {
                        storageSingleton.liquidCompBuildDef = Assets.GetBuildingDef(LiquidQuantumCompressorConfig.ID);
                    }
                    Assets.BuildingDefs.Remove(storageSingleton.liquidCompBuildDef);
                    break;
            }
        }
        protected override void OnCleanUp()
        {
            //PlayerController.Instance.ToolDeactivated(BuildTool.Instance);
            QuantumStorageSingleton storageSingleton = QuantumStorageSingleton.Get();
            if ((GetComponent<BuildingComplete>()!= null&& GetComponent<BuildingUnderConstruction>() == null)||(GetComponent<BuildingComplete>() == null && GetComponent<BuildingUnderConstruction>() == null))
            {
                switch (conduitType)
                {
                    case ConduitType.Gas:
                        Assets.BuildingDefs.Add(storageSingleton.gasCompBuildDef);
                        break;
                    case ConduitType.Liquid:
                        Assets.BuildingDefs.Add(storageSingleton.liquidCompBuildDef);
                        break;
                }
            }
            
            base.OnCleanUp();
        }
    }
}
