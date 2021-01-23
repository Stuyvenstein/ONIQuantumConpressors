using QuantumCompressors.BuildingComponents;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUNING;
using UnityEngine;

namespace QuantumCompressors.BuildingConfigs.Gas
{

    public class GasQuantumCompressorConfig : IBuildingConfig
    {
        public static string UPPERID { get { return ID.ToUpperInvariant(); } }
        public const string ID = "GasQuantumCompressor";
        public const string NAME = "Gas Quantum Compressor";
        public static string DESC = "Uses quantum compression to store large amounts of " + UI.FormatAsLink("gases", "ELEMENTS_GAS") + ", and with quantum entanglement, storage management becomes a breeze.";
        public static string PORT_ID = "GasQuantumCompressorLogicPort";
        private const ConduitType conduitType = ConduitType.Gas;


        public override BuildingDef CreateBuildingDef()
        {
            //List<string> constrMats = new List<string>();
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 5, 3, "gasstorage_kanim", 100, 120f, QuantumStorage.CompressorCost, QuantumStorage.CompressorMats, 1000f, BuildLocationRule.OnFloor, TUNING.BUILDINGS.DECOR.PENALTY.TIER1, NOISE_POLLUTION.NOISY.TIER0, 0.2f);
            buildingDef.Floodable = false;
            //buildingDef.InputConduitType = inputPort.conduitType;
            //buildingDef.OutputConduitType = outputPort.conduitType;
            buildingDef.ViewMode = OverlayModes.GasConduits.ID;
            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = QuantumStorage.QuantumStoragePowerConsume;
            buildingDef.AudioCategory = "HollowMetal";
            buildingDef.RequiredDlcId = "EXPANSION1_ID";
            //buildingDef.OnePerWorld = true;
            //buildingDef.UtilityInputOffset = inputPort.offset;
            //buildingDef.UtilityOutputOffset = outputPort.offset;
            List<LogicPorts.Port> list = new List<LogicPorts.Port>();
            list.Add(LogicPorts.Port.OutputPort(PORT_ID, new CellOffset(0, 0), STRINGS.BUILDINGS.PREFABS.SMARTRESERVOIR.LOGIC_PORT, STRINGS.BUILDINGS.PREFABS.SMARTRESERVOIR.LOGIC_PORT_ACTIVE, STRINGS.BUILDINGS.PREFABS.SMARTRESERVOIR.LOGIC_PORT_INACTIVE, false, false));
            buildingDef.LogicOutputPorts = list;
            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, ID);
            return buildingDef;
        }
        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<Reservoir>();
            go.GetComponent<KPrefabID>().AddTag(GameTags.UniquePerWorld, false);
            Storage storage = BuildingTemplates.CreateDefaultStorage(go, false);
            storage.showDescriptor = true;
            storage.storageFilters = STORAGEFILTERS.GASES;
            storage.capacityKg = (150f * QuantumStorage.QuantumStorageMultiplier);
            List<Storage.StoredItemModifier> ReservoirStoredItemModifiers = new List<Storage.StoredItemModifier>();
            ReservoirStoredItemModifiers.Add(Storage.StoredItemModifier.Hide);
            ReservoirStoredItemModifiers.Add(Storage.StoredItemModifier.Seal);
            storage.SetDefaultStoredItemModifiers(ReservoirStoredItemModifiers);
            storage.showCapacityStatusItem = true;
            storage.showCapacityAsMainStatus = true;
            go.AddOrGet<SmartReservoir>();
            var qs = go.AddOrGet<QuantumStorage>();
            qs.conduitType = conduitType;
        }
        public override void DoPostConfigureComplete(GameObject go)
        {
            //UnityEngine.Object.DestroyImmediate(go.GetComponent<ConduitConsumer>());
            go.AddOrGetDef<StorageController.Def>();
            RequireInputs component = go.GetComponent<RequireInputs>();
            component.SetRequirements(true, false);
            //go.AddOrGetDef<PoweredActiveController.Def>().showWorkingStatus = true;
            go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayBehindConduits, false);
        }

    }
}
