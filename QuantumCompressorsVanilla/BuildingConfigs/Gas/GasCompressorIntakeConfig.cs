﻿using QuantumCompressors.BuildingComponents;
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
			
			GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, ID);
			return buildingDef;

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
