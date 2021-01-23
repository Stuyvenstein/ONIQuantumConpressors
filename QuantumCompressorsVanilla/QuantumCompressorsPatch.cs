using Harmony;
using QuantumCompressors.BuildingComponents;
using QuantumCompressors.BuildingConfigs.Gas;
using QuantumCompressors.BuildingConfigs.Liquid;
using QuantumCompressors.BuildingConfigs.Solid;
using System.Collections.Generic;
using UnityEngine;

namespace QuantumCompressors
{
    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    internal class QuantumCompressorsPreLoad
    {
        private static void Prefix() 
        {
            QCModUtils.AddStructure("Base", GasQuantumCompressorConfig.ID, GasQuantumCompressorConfig.NAME, GasQuantumCompressorConfig.DESC, GasQuantumCompressorConfig.DESC);
            QCModUtils.AddStructure("Base", LiquidQuantumCompressorConfig.ID, LiquidQuantumCompressorConfig.NAME, LiquidQuantumCompressorConfig.DESC, LiquidQuantumCompressorConfig.DESC);
            //TODO Finish Solids Compressor
            //QCModUtils.AddStructure("Base", SolidQuantumCompressorConfig.ID, SolidQuantumCompressorConfig.NAME, SolidQuantumCompressorConfig.DESC, SolidQuantumCompressorConfig.DESC);

            QCModUtils.AddStructure("HVAC", GasCompressorIntakeConfig.ID, GasCompressorIntakeConfig.NAME, GasCompressorIntakeConfig.DESC, GasCompressorIntakeConfig.DESC);
            QCModUtils.AddStructure("HVAC", GasQuantumFilterOutletConfig.ID, GasQuantumFilterOutletConfig.NAME, GasQuantumFilterOutletConfig.DESC, GasQuantumFilterOutletConfig.DESC);
            QCModUtils.AddStructure("HVAC", GasQuantumFilterDualConfig.ID, GasQuantumFilterDualConfig.NAME, GasQuantumFilterDualConfig.DESC, GasQuantumFilterDualConfig.DESC);

            QCModUtils.AddStructure("Plumbing", LiquidCompressorIntakeConfig.ID, LiquidCompressorIntakeConfig.NAME, LiquidCompressorIntakeConfig.DESC, LiquidCompressorIntakeConfig.DESC);
            QCModUtils.AddStructure("Plumbing", LiquidQuantumFilterOutletConfig.ID, LiquidQuantumFilterOutletConfig.NAME, LiquidQuantumFilterOutletConfig.DESC, LiquidQuantumFilterOutletConfig.DESC);
            QCModUtils.AddStructure("Plumbing", LiquidQuantumFilterDualConfig.ID, LiquidQuantumFilterDualConfig.NAME, LiquidQuantumFilterDualConfig.DESC, LiquidQuantumFilterDualConfig.DESC);
        }


    }

    [HarmonyPatch(typeof(Db), "Initialize")]
    internal class QuantumCompressorsDbPostInit
    {
        private static void Postfix()
        {
            var db = Db.Get();
            if (db != null)
            {
                QCModUtils.AddStructureTech(db, "DupeTrafficControl", GasQuantumCompressorConfig.ID);
                QCModUtils.AddStructureTech(db, "DupeTrafficControl", LiquidQuantumCompressorConfig.ID);
                //QCModUtils.AddStructureTech(db, "DupeTrafficControl", SolidQuantumCompressorConfig.ID);

                QCModUtils.AddStructureTech(db, "DupeTrafficControl", GasQuantumFilterOutletConfig.ID);
                QCModUtils.AddStructureTech(db, "DupeTrafficControl", GasCompressorIntakeConfig.ID);
                QCModUtils.AddStructureTech(db, "DupeTrafficControl", GasQuantumFilterDualConfig.ID);

                QCModUtils.AddStructureTech(db, "DupeTrafficControl", LiquidQuantumFilterOutletConfig.ID);                
                QCModUtils.AddStructureTech(db, "DupeTrafficControl", LiquidCompressorIntakeConfig.ID);
                QCModUtils.AddStructureTech(db, "DupeTrafficControl", LiquidQuantumFilterDualConfig.ID);
            }
        }
    }

    //Patch to show filter side screen on QuantumElementFilter
    [HarmonyPatch(typeof(FilterSideScreen), "IsValidForTarget")]
    internal class QuantumElementFilterSideScreen
    {
        static bool Prefix(FilterSideScreen __instance,ref bool __result, GameObject target)
        {

            bool isValid;
            if (!__instance.isLogicFilter)
            {
                isValid = (target.GetComponent<QuantumElementFilter>() != null);
                if (isValid)
                {
                    __result = isValid && target.GetComponent<Filterable>() != null;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BaseUtilityBuildTool), "CheckForConnection")]
    internal class QuantumElementFilterOverrides
    {

        static bool Prefix(ref bool __result, int cell, string defName, string soundName, ref BuildingCellVisualizer outBcv, bool fireEvents = true)
        {
            outBcv = null;
            GameObject gameObject = Grid.Objects[cell, 1];
            Building building = null;
            bool isQFilter = false;
            if (gameObject != null)
            {
                building = gameObject.GetComponent<Building>();
            }
            if (!building) {
                __result = false;
                return false;
            }
            int elemFiltOutCellNum = -1;
            if (defName.Contains("Liquid") || defName.Contains("Gas"))
            {
                QuantumElementFilter qElemFiltComp = building.GetComponent<QuantumElementFilter>();
                if (qElemFiltComp != null)
                {
                    isQFilter = true;
                    if (qElemFiltComp.outPortInfo.conduitType == ConduitType.Liquid || qElemFiltComp.outPortInfo.conduitType == ConduitType.Gas)
                    {
                        elemFiltOutCellNum = qElemFiltComp.GetFilteredCell();
                    }
                }
            }
            if (cell == elemFiltOutCellNum && isQFilter)
            {
                BuildingCellVisualizer bcvComp = building.gameObject.GetComponent<BuildingCellVisualizer>();
                outBcv = bcvComp;
                if (bcvComp != null)
                {
                    if (fireEvents)
                    {
                        bcvComp.ConnectedEvent(cell);
                        string sound = GlobalAssets.GetSound(soundName, false);
                        if (sound != null)
                        {
                            KMonoBehaviour.PlaySound(sound);
                        }
                    }
                    __result = true;
                    return false;
                }
            }
            outBcv = null;
            return true;
        }
    }
}

