using System;
using System.Collections.Generic;
using QuantumCompressors.BuildingComponents;
using STRINGS;
using TUNING;
using UnityEngine;

namespace QuantumCompressors.BuildingConfigs.Solid
{
    public class SolidQuantumFilterDualConfig:IBuildingConfig
    {
		private const ConduitType conduitType = ConduitType.Solid;
		public static string UPPERID { get { return ID.ToUpperInvariant(); } }
		public const string NAME = "Quantum Conveyor Intake/Outlet";
		public static string DESC = "Stores " + STRINGS.UI.FormatAsLink("solids", "ELEMENTS_SOLID") + " in a " + STRINGS.UI.FormatAsLink(SolidQuantumCompressorConfig.NAME, SolidQuantumCompressorConfig.UPPERID) + ", and receives a user-selected " + STRINGS.UI.FormatAsLink("solid", "ELEMENTS_SOLID") + " type from a " + STRINGS.UI.FormatAsLink(SolidQuantumCompressorConfig.NAME, SolidQuantumCompressorConfig.UPPERID) + ".";
		public const string ID = "SolidQuantumFilterDual";
		private ConduitPortInfo outputPort = new ConduitPortInfo(conduitType, new CellOffset(0, 1));
		private ConduitPortInfo inputPort = new ConduitPortInfo(conduitType, new CellOffset(0, 0));
		public override BuildingDef CreateBuildingDef()
		{
			int width = 1;
			int height = 2;
			string anim = "conveyor_shutoff_kanim";
			int hitpoints = 30;
			float construction_time = 10f;
			float[] tier = TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2;
			string[] refined_METALS = MATERIALS.REFINED_METALS;
			float melting_point = 1600f;
			BuildLocationRule build_location_rule = BuildLocationRule.Anywhere;
			EffectorValues tier2 = NOISE_POLLUTION.NOISY.TIER1;
			BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, width, height, anim, hitpoints, construction_time, QuantumStorage.DualPortCost, QuantumStorage.DualPortMats, melting_point, build_location_rule, TUNING.BUILDINGS.DECOR.PENALTY.TIER0, tier2, 0.2f);
			buildingDef.InputConduitType = conduitType;
			buildingDef.OutputConduitType = conduitType;
			buildingDef.Floodable = false;
			buildingDef.Entombable = false;
			buildingDef.Overheatable = false;
			buildingDef.ViewMode = OverlayModes.SolidConveyor.ID;
			buildingDef.AudioCategory = "Metal";
			buildingDef.AudioSize = "small";
			buildingDef.BaseTimeUntilRepair = -1f;
			buildingDef.PermittedRotations = PermittedRotations.R360;
			buildingDef.UtilityInputOffset = inputPort.offset;
			buildingDef.UtilityOutputOffset = outputPort.offset;
			buildingDef.RequiresPowerInput = true;
			buildingDef.EnergyConsumptionWhenActive = 20f;
			buildingDef.PowerInputOffset = outputPort.offset;
			List<LogicPorts.Port> list = new List<LogicPorts.Port>();
			//list.Add(LogicPorts.Port.InputPort(LogicOperationalController.PORT_ID, new CellOffset(0, 0), STRINGS.BUILDINGS.PREFABS.SOLIDLOGICVALVE.LOGIC_PORT, STRINGS.BUILDINGS.PREFABS.SOLIDLOGICVALVE.LOGIC_PORT_ACTIVE, STRINGS.BUILDINGS.PREFABS.SOLIDLOGICVALVE.LOGIC_PORT_INACTIVE, true, false));
			buildingDef.LogicInputPorts = list;
			GeneratedBuildings.RegisterWithOverlay(OverlayScreen.SolidConveyorIDs, ID);
			return buildingDef;
		}

		// Token: 0x06000CA3 RID: 3235 RVA: 0x00003E83 File Offset: 0x00002083
		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			
			QuantumElementFilter elementFilter = go.AddOrGet<QuantumElementFilter>();
			elementFilter.outPortInfo = outputPort;
			elementFilter.inPortInfo = inputPort;
			elementFilter.useInput = true;
			Filterable filterable = go.AddOrGet<Filterable>();
			filterable.filterElementState = Filterable.ElementState.Solid;
			
		}

		// Token: 0x06000CA4 RID: 3236 RVA: 0x00048CC8 File Offset: 0x00046EC8
		public override void DoPostConfigureComplete(GameObject go)
		{
			UnityEngine.Object.DestroyImmediate(go.GetComponent<ConduitConsumer>());
			UnityEngine.Object.DestroyImmediate(go.GetComponent<ConduitDispenser>());
			UnityEngine.Object.DestroyImmediate(go.GetComponent<RequireOutputs>());
			go.AddOrGetDef<PoweredActiveController.Def>().showWorkingStatus = true;
			RequireInputs component = go.GetComponent<RequireInputs>();
			component.SetRequirements(true, false);
			go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits, false);
			//LogicOperationalController logicOperationalController = go.AddOrGet<LogicOperationalController>();
			//logicOperationalController.unNetworkedValue = 0;
			//RequireOutputs requireOutputs = go.AddOrGet<RequireOutputs>();
			//requireOutputs.ignoreFullPipe = true;
			//go.AddOrGet<SolidConduitBridge>();
			//go.AddOrGet<SolidLogicValve>();
		}

	}
}
