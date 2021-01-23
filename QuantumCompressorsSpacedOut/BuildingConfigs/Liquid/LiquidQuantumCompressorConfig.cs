using QuantumCompressors.BuildingComponents;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUNING;
using UnityEngine;

namespace QuantumCompressors.BuildingConfigs.Liquid
{
    public class LiquidQuantumCompressorConfig:IBuildingConfig
    {

		public static string UPPERID { get { return ID.ToUpperInvariant(); } }
		public const string ID = "LiquidQuantumCompressor";
		public const string NAME = "Liquid Quantum Compressor";
		public static string DESC = "Uses quantum compression to store large amounts of " + UI.FormatAsLink("liquids", "ELEMENTS_LIQUID") + ", and with quantum entanglement, storage management becomes a breeze.";
		public static string PORT_ID = "LiquidQuantumCompressorLogicPort";
		private const ConduitType conduitType = ConduitType.Liquid;
		//private ConduitPortInfo outputPort = new ConduitPortInfo(conduitType, new CellOffset(0,0));
		//private ConduitPortInfo inputPort = new ConduitPortInfo(conduitType, new CellOffset(1,2));
		private Storage storage;
		public override BuildingDef CreateBuildingDef()
		{
			BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 2, 3, "liquidreservoir_kanim", 100, 120f, QuantumStorage.CompressorCost, QuantumStorage.CompressorMats, 1000f, BuildLocationRule.OnFloor, TUNING.BUILDINGS.DECOR.PENALTY.TIER1, NOISE_POLLUTION.NOISY.TIER0, 0.2f);
            //buildingDef.InputConduitType = inputPort.conduitType;
            //buildingDef.OutputConduitType = outputPort.conduitType;
            buildingDef.Floodable = false;
			buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
			buildingDef.AudioCategory = "HollowMetal";
			buildingDef.RequiresPowerInput = true;
			buildingDef.RequiredDlcId = "EXPANSION1_ID";
			buildingDef.EnergyConsumptionWhenActive = QuantumStorage.QuantumStoragePowerConsume;
			//buildingDef.OnePerWorld = true;
			//buildingDef.UtilityInputOffset = inputPort.offset;
			//buildingDef.UtilityOutputOffset = outputPort.offset;
			List<LogicPorts.Port> lPortList = new List<LogicPorts.Port>();
			lPortList.Add(LogicPorts.Port.OutputPort(PORT_ID, new CellOffset(0, 0), STRINGS.BUILDINGS.PREFABS.SMARTRESERVOIR.LOGIC_PORT, STRINGS.BUILDINGS.PREFABS.SMARTRESERVOIR.LOGIC_PORT_ACTIVE, STRINGS.BUILDINGS.PREFABS.SMARTRESERVOIR.LOGIC_PORT_INACTIVE, false, false));
			buildingDef.LogicOutputPorts = lPortList;
			GeneratedBuildings.RegisterWithOverlay(OverlayScreen.LiquidVentIDs, ID);
			return buildingDef;
		}

		// Token: 0x06000860 RID: 2144 RVA: 0x00032F38 File Offset: 0x00031138
		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			go.AddOrGet<Reservoir>();
			go.GetComponent<KPrefabID>().AddTag(GameTags.UniquePerWorld, false);
			Storage storage = BuildingTemplates.CreateDefaultStorage(go, false);
			storage.showDescriptor = true;
			storage.allowItemRemoval = false;
			storage.storageFilters = STORAGEFILTERS.LIQUIDS;
			storage.capacityKg = (5000f*QuantumStorage.QuantumStorageMultiplier);
			storage.SetDefaultStoredItemModifiers(GasReservoirConfig.ReservoirStoredItemModifiers);
			storage.showCapacityStatusItem = true;
			storage.showCapacityAsMainStatus = true;
			this.storage = storage;
			go.AddOrGet<SmartReservoir>();
			var qs = go.AddOrGet<QuantumStorage>();
			qs.conduitType = conduitType;
		}

		// Token: 0x06000861 RID: 2145 RVA: 0x0002B3BB File Offset: 0x000295BB
		public override void DoPostConfigureComplete(GameObject go)
		{
            //UnityEngine.Object.DestroyImmediate()
			go.AddOrGetDef<StorageController.Def>();
			RequireInputs component = go.GetComponent<RequireInputs>();
			component.SetRequirements(true, false);
			go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayBehindConduits, false);
		}
		
	}
}
