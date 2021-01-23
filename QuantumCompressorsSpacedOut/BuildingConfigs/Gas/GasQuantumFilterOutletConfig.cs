
using QuantumCompressors.BuildingComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUNING;
using UnityEngine;
using static STRINGS.UI.OVERLAYS;

namespace QuantumCompressors.BuildingConfigs.Gas
{
    public class GasQuantumFilterOutletConfig : IBuildingConfig
    {
		//public const string ID = "GasCompressorFilterOutlet";
		public static string UPPERID { get { return ID.ToUpperInvariant(); } }
		public const string NAME = "Quantum Gas Outlet";
		public static string DESC = "Receives a user-selected " + STRINGS.UI.FormatAsLink("gas", "ELEMENTS_GAS") + " type from a " + STRINGS.UI.FormatAsLink(GasQuantumCompressorConfig.NAME, GasQuantumCompressorConfig.UPPERID) + ".";
		public const string ID = "GasQuantumFilterOutlet";
		private const ConduitType conduitType = ConduitType.Gas;
		private ConduitPortInfo outputPort = new ConduitPortInfo(conduitType, new CellOffset(0, 1));
		//private ConduitPortInfo inputPort = new ConduitPortInfo(conduitType, new CellOffset(0, 1));
		public static string EFFECT { get { return DESC; } }
		public override BuildingDef CreateBuildingDef()
		{
			BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "valvegas_logic_kanim", 30, 10f, QuantumStorage.OutletCost, QuantumStorage.OutletMats,
				1600f, BuildLocationRule.Anywhere, TUNING.BUILDINGS.DECOR.PENALTY.TIER0, TUNING.NOISE_POLLUTION.NOISY.TIER1, 0.2f);
			//buildingDef.InputConduitType = inputPort.conduitType;
			buildingDef.OutputConduitType = outputPort.conduitType;
			buildingDef.Floodable = false;
            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 10f;
            buildingDef.PowerInputOffset = outputPort.offset;
            buildingDef.ViewMode = OverlayModes.GasConduits.ID;
			buildingDef.AudioCategory = "Metal";
			buildingDef.PermittedRotations = PermittedRotations.R360;
			//buildingDef.UtilityInputOffset = inputPort.offset;
			buildingDef.UtilityOutputOffset = outputPort.offset;
			//List<LogicPorts.Port> list = new List<LogicPorts.Port>();
			//list.Add(LogicPorts.Port.InputPort(LogicOperationalController.PORT_ID, new CellOffset(0, 0), STRINGS.BUILDINGS.PREFABS.GASLOGICVALVE.LOGIC_PORT, STRINGS.BUILDINGS.PREFABS.GASLOGICVALVE.LOGIC_PORT_ACTIVE, STRINGS.BUILDINGS.PREFABS.GASLOGICVALVE.LOGIC_PORT_INACTIVE, true, false));
			//buildingDef.LogicInputPorts = list;
			GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, ID);
			return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
			//UnityEngine.Object.DestroyImmediate(go.GetComponent<ConduitDispenser>());
			//go.AddOrGet<Structure>();
		}
		public override void DoPostConfigureComplete(GameObject go)
		{
			//UnityEngine.Object.DestroyImmediate(go.GetComponent<ConduitConsumer>());
			UnityEngine.Object.DestroyImmediate(go.GetComponent<ConduitDispenser>());
			//UnityEngine.Object.DestroyImmediate(go.GetComponent<RequireInputs>());
			QuantumElementFilter elementFilter = go.AddOrGet<QuantumElementFilter>();
			elementFilter.outPortInfo = outputPort;
			//elementFilter.inPortInfo =inputPort;
			elementFilter.useInput = false;
			Filterable filterable = go.AddOrGet<Filterable>();
			filterable.filterElementState = Filterable.ElementState.Gas;
			go.AddOrGetDef<PoweredActiveController.Def>().showWorkingStatus = true;
			RequireInputs component = go.GetComponent<RequireInputs>();
            component.SetRequirements(true, false);
            //RequireInputs component = go.GetComponent<RequireInputs>();
            //component.SetRequirements(true, false);
            go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits, false);
			
		}
	}
}
