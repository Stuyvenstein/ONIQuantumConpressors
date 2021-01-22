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
    public class LiquidCompressorIntakeConfig:IBuildingConfig
    {
		public const string ID = "LiquidCompressorIntake";
		public static string UPPERID { get { return ID.ToUpperInvariant(); } }
		public const string NAME = "Quantum Liquid Intake";
		public static string DESC = "Receives " + UI.FormatAsLink("liquid", "ELEMENTS_LIQUID") + " and entangles it into a " + UI.FormatAsLink(LiquidQuantumCompressorConfig.NAME, LiquidQuantumCompressorConfig.UPPERID) + ".";
		private ConduitPortInfo outputPort = new ConduitPortInfo(ConduitType.Liquid, new CellOffset(0, 0));
		
		public override BuildingDef CreateBuildingDef()
		{

			BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "valveliquid_logic_kanim", 30, 10f, QuantumStorage.IntakeCost, QuantumStorage.IntakeMats,
				1600f, BuildLocationRule.Anywhere, TUNING.BUILDINGS.DECOR.PENALTY.TIER0, TUNING.NOISE_POLLUTION.NOISY.TIER1, 0.2f);
			buildingDef.InputConduitType = outputPort.conduitType;
			//buildingDef.OutputConduitType = secondaryPort.conduitType;
			buildingDef.Floodable = false;
            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 10f;
            buildingDef.PowerInputOffset = outputPort.offset;
            buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
			buildingDef.AudioCategory = "Metal";
			buildingDef.PermittedRotations = PermittedRotations.R360;
			buildingDef.UtilityInputOffset = outputPort.offset;
			//buildingDef.UtilityOutputOffset = secondaryPort.offset;
			List<LogicPorts.Port> list = new List<LogicPorts.Port>();
			//list.Add(LogicPorts.Port.InputPort(LogicOperationalController.PORT_ID, new CellOffset(0, 0), STRINGS.BUILDINGS.PREFABS.GASLOGICVALVE.LOGIC_PORT, STRINGS.BUILDINGS.PREFABS.GASLOGICVALVE.LOGIC_PORT_ACTIVE, STRINGS.BUILDINGS.PREFABS.GASLOGICVALVE.LOGIC_PORT_INACTIVE, true, false));
			buildingDef.LogicInputPorts = list;
			GeneratedBuildings.RegisterWithOverlay(OverlayScreen.LiquidVentIDs, ID);
			return buildingDef;
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			//UnityEngine.Object.DestroyImmediate(go.GetComponent<RequireInputs>());
			UnityEngine.Object.DestroyImmediate(go.GetComponent<ConduitConsumer>());
			QuantumConduitConsumer conduitConsumer = go.AddOrGet<QuantumConduitConsumer>();
			conduitConsumer.conduitType = outputPort.conduitType;
			conduitConsumer.ignoreMinMassCheck = true;
			conduitConsumer.forceAlwaysSatisfied = true;

			//RequireInputs component = go.GetComponent<RequireInputs>();
			//component.SetRequirements(true, false);
			//conduitConsumer.alwaysConsume = true;
			//conduitConsumer.capacityKG = 1000f;
			go.AddOrGetDef<PoweredActiveController.Def>().showWorkingStatus = true;
			RequireInputs component = go.GetComponent<RequireInputs>();
            component.SetRequirements(true, false);
            go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits, false);
		}
	}
}
