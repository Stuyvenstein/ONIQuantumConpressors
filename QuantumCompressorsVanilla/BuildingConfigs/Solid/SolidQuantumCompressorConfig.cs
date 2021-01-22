using QuantumCompressors.BuildingComponents;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUNING;
using UnityEngine;

namespace QuantumCompressors.BuildingConfigs.Solid
{
    public class SolidQuantumCompressorConfig:IBuildingConfig
    {
		public static string UPPERID { get { return ID.ToUpperInvariant(); } }
		public const string ID = "SolidQuantumCompressor";
		public const string NAME = "Solid Quantum Compressor";
		public static string DESC = "Uses quantum compression to store large amounts of " + UI.FormatAsLink("solids", "ELEMENTS_SOLID") + " and with quantum entanglement, storage management becomes a breeze.";
		public static string PORT_ID = "SolidQuantumCompressorLogicPort";
		private const ConduitType conduitType = ConduitType.Solid;
		public override BuildingDef CreateBuildingDef()
		{
			int width = 1;
			int height = 2;
			string anim = "smartstoragelocker_kanim";
			int hitpoints = 30;
			float construction_time = 120f;
			//float[] tier = TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3;
			//string[] refined_METALS = MATERIALS.REFINED_METALS;
			float melting_point = 1000f;
			BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
			EffectorValues none = NOISE_POLLUTION.NONE;
			BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, width, height, anim, hitpoints, construction_time, QuantumStorage.CompressorCost, QuantumStorage.CompressorMats, melting_point, build_location_rule, TUNING.BUILDINGS.DECOR.PENALTY.TIER1, none, 0.2f);
			buildingDef.Floodable = false;
			buildingDef.AudioCategory = "Metal";
			buildingDef.Overheatable = false;
			//buildingDef.RequiredDlcId = "EXPANSION1_ID";
			buildingDef.ViewMode = OverlayModes.Logic.ID;
			buildingDef.RequiresPowerInput = true;
			buildingDef.EnergyConsumptionWhenActive = QuantumStorage.QuantumStoragePowerConsume;
			buildingDef.ExhaustKilowattsWhenActive = 0.125f;
			List<LogicPorts.Port> list = new List<LogicPorts.Port>();
			list.Add(LogicPorts.Port.OutputPort(FilteredStorage.FULL_PORT_ID, new CellOffset(0, 1), STRINGS.BUILDINGS.PREFABS.STORAGELOCKERSMART.LOGIC_PORT, STRINGS.BUILDINGS.PREFABS.STORAGELOCKERSMART.LOGIC_PORT_ACTIVE, STRINGS.BUILDINGS.PREFABS.STORAGELOCKERSMART.LOGIC_PORT_INACTIVE, true, false));
			buildingDef.LogicOutputPorts = list;
			return buildingDef;
		}

		// Token: 0x06000CDA RID: 3290 RVA: 0x00049D2C File Offset: 0x00047F2C
		public override void DoPostConfigureComplete(GameObject go)
		{
			SoundEventVolumeCache.instance.AddVolume("storagelocker_kanim", "StorageLocker_Hit_metallic_low", NOISE_POLLUTION.NOISY.TIER1);
			Prioritizable.AddRef(go);
			//go.GetComponent<KPrefabID>().AddTag(GameTags.UniquePerWorld, false);
			Storage storage = go.AddOrGet<Storage>();
			storage.capacityKg = (20000f * QuantumStorage.QuantumSolidStorageMultiplier);
			storage.showInUI = true;
			storage.allowItemRemoval = true;
			storage.showDescriptor = true;
			storage.storageFilters = STORAGEFILTERS.NOT_EDIBLE_SOLIDS;
			storage.storageFullMargin = STORAGE.STORAGE_LOCKER_FILLED_MARGIN;
			storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
			//storage.showCapacityStatusItem = true;
			//storage.showCapacityAsMainStatus = true;
			CopyBuildingSettings copyBuildingSettings = go.AddOrGet<CopyBuildingSettings>();
			copyBuildingSettings.copyGroupTag = GameTags.StorageLocker;
			go.AddOrGet<StorageLockerSmart>();
			go.AddOrGet<UserNameable>();
			go.AddOrGetDef<StorageController.Def>();
			var qs = go.AddComponent<QuantumStorage>();
			qs.conduitType = conduitType;
		}

		// Token: 0x040006A0 RID: 1696
		//public const string ID = "StorageLockerSmart";
		//public override BuildingDef CreateBuildingDef()
		//{
		//	BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 5, 3, "gasstorage_kanim", 100, 120f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.ALL_METALS, 800f, BuildLocationRule.OnFloor, TUNING.BUILDINGS.DECOR.PENALTY.TIER1, NOISE_POLLUTION.NOISY.TIER0, 0.2f);
		//	buildingDef.Floodable = false;
		//	buildingDef.InputConduitType = conduitType;
		//	buildingDef.OutputConduitType = conduitType;
		//	buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
		//	buildingDef.AudioCategory = "HollowMetal";
		//	buildingDef.OnePerWorld = true;
		//	buildingDef.UtilityInputOffset = new CellOffset(1, 2);
		//	buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
		//	List<LogicPorts.Port> list = new List<LogicPorts.Port>();
		//	list.Add(LogicPorts.Port.OutputPort(PORT_ID, new CellOffset(0, 0), STRINGS.BUILDINGS.PREFABS.SMARTRESERVOIR.LOGIC_PORT, STRINGS.BUILDINGS.PREFABS.SMARTRESERVOIR.LOGIC_PORT_ACTIVE, STRINGS.BUILDINGS.PREFABS.SMARTRESERVOIR.LOGIC_PORT_INACTIVE, false, false));
		//	buildingDef.LogicOutputPorts = list;
		//	GeneratedBuildings.RegisterWithOverlay(OverlayScreen.SolidConveyorIDs, ID);
		//	return buildingDef;
		//}
		//public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		//{
		//	go.AddOrGet<Reservoir>();
		//	go.GetComponent<KPrefabID>().AddTag(GameTags.UniquePerWorld, false);
		//	Storage storage = BuildingTemplates.CreateDefaultStorage(go, false);
		//	storage.showDescriptor = true;
		//	storage.storageFilters = STORAGEFILTERS.NOT_EDIBLE_SOLIDS;
		//	storage.capacityKg = 1500f;
		//	List<Storage.StoredItemModifier> ReservoirStoredItemModifiers = new List<Storage.StoredItemModifier>();
		//	ReservoirStoredItemModifiers.Add(Storage.StoredItemModifier.Hide);
		//	ReservoirStoredItemModifiers.Add(Storage.StoredItemModifier.Seal);
		//	storage.SetDefaultStoredItemModifiers(ReservoirStoredItemModifiers);
		//	storage.showCapacityStatusItem = true;
		//	storage.showCapacityAsMainStatus = true;
		//	go.AddOrGet<SmartReservoir>();
		//	ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
		//	conduitConsumer.conduitType = conduitType;
		//	conduitConsumer.ignoreMinMassCheck = true;
		//	conduitConsumer.forceAlwaysSatisfied = true;
		//	conduitConsumer.alwaysConsume = true;
		//	conduitConsumer.capacityKG = storage.capacityKg;
		//	ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
		//	conduitDispenser.conduitType = conduitType;
		//	conduitDispenser.elementFilter = null;
		//	var qs = go.AddOrGet<QuantumStorage>();
		//	qs.conduitType = conduitType;
		//}
		//public override void DoPostConfigureComplete(GameObject go)
		//{
		//	go.AddOrGetDef<StorageController.Def>();
		//	go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayBehindConduits, false);
		//}
	}
}
