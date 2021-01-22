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
    public class GasCompressorIntakeConfig : IBuildingConfig
    {

		public const string ID = "GasCompressorIntake";
		public static string UPPERID { get { return ID.ToUpperInvariant(); } }
		public const string NAME = "Quantum Gas Intake";
		public static string DESC = "Receives "+UI.FormatAsLink("gas", "ELEMENTS_GAS") +" and entangles it into a "+UI.FormatAsLink(GasQuantumCompressorConfig.NAME, GasQuantumCompressorConfig.UPPERID)+".";
		private ConduitPortInfo outputPort = new ConduitPortInfo(ConduitType.Gas, new CellOffset(0, 0));
		//public static LocString LOGIC_PORT = "Close/Open";
		//public static LocString LOGIC_PORT_ACTIVE = UI.FormatAsAutomationState("Red Signal", UI.AutomationState.Standby) + ": Allow gas flow";
		//public static LocString LOGIC_PORT_INACTIVE = UI.FormatAsAutomationState("Green Signal", UI.AutomationState.Active) + ": Prevent gas flow";

		public override BuildingDef CreateBuildingDef()
        {
			
			BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "valvegas_logic_kanim", 30, 10f, QuantumStorage.IntakeCost, QuantumStorage.IntakeMats,
				1600f, BuildLocationRule.Anywhere, TUNING.BUILDINGS.DECOR.PENALTY.TIER0, TUNING.NOISE_POLLUTION.NOISY.TIER1, 0.2f);
			buildingDef.InputConduitType = outputPort.conduitType;
			//buildingDef.OutputConduitType = secondaryPort.conduitType;
			buildingDef.Floodable = false;
            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 10f;
            buildingDef.PowerInputOffset = outputPort.offset;
            buildingDef.ViewMode = OverlayModes.GasConduits.ID;
			buildingDef.AudioCategory = "Metal";
			buildingDef.PermittedRotations = PermittedRotations.R360;
			buildingDef.UtilityInputOffset= outputPort.offset;
			//buildingDef.UtilityOutputOffset = secondaryPort.offset;
			//List<LogicPorts.Port> list = new List<LogicPorts.Port>();
			//list.Add(LogicPorts.Port.InputPort(LogicOperationalController.PORT_ID, new CellOffset(0, 0), STRINGS.BUILDINGS.PREFABS.GASLOGICVALVE.LOGIC_PORT, STRINGS.BUILDINGS.PREFABS.GASLOGICVALVE.LOGIC_PORT_ACTIVE, STRINGS.BUILDINGS.PREFABS.GASLOGICVALVE.LOGIC_PORT_INACTIVE, true, false));
			//buildingDef.LogicInputPorts = list;
			GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, ID);
			return buildingDef;


			//BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "valvegas_logic_kanim", 30, 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER1, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.Anywhere, TUNING.BUILDINGS.DECOR.PENALTY.TIER0, NOISE_POLLUTION.NOISY.TIER1, 0.2f);
			//buildingDef.InputConduitType = ConduitType.Gas;
			////buildingDef.OutputConduitType = ConduitType.Gas;
			//buildingDef.Floodable = false;
			//buildingDef.RequiresPowerInput = true;
			//buildingDef.EnergyConsumptionWhenActive = 10f;
			//buildingDef.PowerInputOffset = new CellOffset(0, 1);
			//buildingDef.ViewMode = OverlayModes.GasConduits.ID;
			//buildingDef.AudioCategory = "Metal";
			//buildingDef.PermittedRotations = PermittedRotations.R360;
			//buildingDef.UtilityInputOffset = new CellOffset(0, 0);
			////buildingDef.UtilityOutputOffset = new CellOffset(0, 1);
			//List<LogicPorts.Port> list = new List<LogicPorts.Port>();
			////list.Add(LogicPorts.Port.InputPort(LogicOperationalController.PORT_ID, new CellOffset(0, 0), LOGIC_PORT, LOGIC_PORT_ACTIVE, LOGIC_PORT_INACTIVE, true, false));
			//buildingDef.LogicInputPorts = list;
			//GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, ID);
			//return buildingDef;
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
			
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			UnityEngine.Object.DestroyImmediate(go.GetComponent<ConduitConsumer>());
			//UnityEngine.Object.DestroyImmediate(go.GetComponent<RequireInputs>());
			QuantumConduitConsumer conduitConsumer = go.AddOrGet<QuantumConduitConsumer>();
			conduitConsumer.conduitType = outputPort.conduitType;
			conduitConsumer.ignoreMinMassCheck = true;
			conduitConsumer.forceAlwaysSatisfied = true;
			//conduitConsumer.alwaysConsume = true;
			//conduitConsumer.capacityKG = 1000f;
			go.AddOrGetDef<PoweredActiveController.Def>().showWorkingStatus = true;
			RequireInputs component = go.GetComponent<RequireInputs>();
            component.SetRequirements(true, false);
            go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits, false);
		}
    }
}
