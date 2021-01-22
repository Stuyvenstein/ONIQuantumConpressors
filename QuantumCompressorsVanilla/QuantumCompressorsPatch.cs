using Harmony;
using QuantumCompressors.BuildingComponents;
using QuantumCompressors.BuildingConfigs.Gas;
using QuantumCompressors.BuildingConfigs.Liquid;
using QuantumCompressors.BuildingConfigs.Solid;
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
    
    [HarmonyPatch(typeof(FilterSideScreen), "IsValidForTarget")]
    internal class QuantumElementFilterSideScreen
    {
        static bool Prefix(FilterSideScreen __instance,ref bool __result, GameObject target)
        {
            bool isValid;
            if (__instance.isLogicFilter)
            {
                isValid = (target.GetComponent<ConduitElementSensor>() != null || target.GetComponent<LogicElementSensor>() != null);
            }
            else
            {
                isValid = (target.GetComponent<ElementFilter>() != null);
                if(!isValid)isValid= (target.GetComponent<QuantumElementFilter>() != null);
            }
            __result= isValid && target.GetComponent<Filterable>() != null;
            return false;
        }
    }

    [HarmonyPatch(typeof(BaseUtilityBuildTool), "CheckForConnection")]
    internal class QuantumElementFilterOverrides
    {

        static bool Prefix(ref bool __result, int cell, string defName, string soundName, ref BuildingCellVisualizer outBcv, bool fireEvents = true)
        {
            outBcv = null;
            DebugUtil.Assert(defName != null, "defName was null");

            GameObject gameObject = Grid.Objects[cell, 1];
            Building building = null;
            if (gameObject != null)
            {
                building = gameObject.GetComponent<Building>();
            }
            if (!building)
            {
                __result = false;
            }
            else
            {
                DebugUtil.Assert(building.gameObject, "targetBuilding.gameObject was null");
                int loginInCellNum = -1;
                int logicOutCellNum = -1;
                int elemFiltOutCellNum = -1;
                if (defName.Contains("LogicWire"))
                {
                    LogicPorts logicPortComponent = building.gameObject.GetComponent<LogicPorts>();
                    if (logicPortComponent != null)
                    {
                        if (logicPortComponent.inputPorts != null)
                        {
                            foreach (ILogicUIElement logicUIElement in logicPortComponent.inputPorts)
                            {
                                DebugUtil.Assert(logicUIElement != null, "input port was null");
                                if (logicUIElement.GetLogicUICell() == cell)
                                {
                                    loginInCellNum = cell;
                                    break;
                                }
                            }
                        }
                        if (loginInCellNum == -1 && logicPortComponent.outputPorts != null)
                        {
                            foreach (ILogicUIElement logicUIElement2 in logicPortComponent.outputPorts)
                            {
                                DebugUtil.Assert(logicUIElement2 != null, "output port was null");
                                if (logicUIElement2.GetLogicUICell() == cell)
                                {
                                    logicOutCellNum = cell;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (defName.Contains("Wire"))
                    {
                        loginInCellNum = building.GetPowerInputCell();
                        logicOutCellNum = building.GetPowerOutputCell();
                    }
                    else
                    {
                        if (defName.Contains("Liquid"))
                        {
                            if (building.Def.InputConduitType == ConduitType.Liquid)
                            {
                                loginInCellNum = building.GetUtilityInputCell();
                            }
                            if (building.Def.OutputConduitType == ConduitType.Liquid)
                            {
                                logicOutCellNum = building.GetUtilityOutputCell();
                            }
                            QuantumElementFilter qElemFiltComp = building.GetComponent<QuantumElementFilter>();
                            if (qElemFiltComp != null)
                            {
                                DebugUtil.Assert(qElemFiltComp.outPortInfo != null, "elementFilter.portInfo was null A");
                                if (qElemFiltComp.outPortInfo.conduitType == ConduitType.Liquid)
                                {
                                    elemFiltOutCellNum = qElemFiltComp.GetFilteredCell();
                                }
                            }
                            else
                            {
                                ElementFilter elemFiltComp = building.GetComponent<ElementFilter>();
                                if (elemFiltComp != null)
                                {
                                    DebugUtil.Assert(elemFiltComp.portInfo != null, "elementFilter.portInfo was null B");
                                    if (elemFiltComp.portInfo.conduitType == ConduitType.Gas)
                                    {
                                        elemFiltOutCellNum = elemFiltComp.GetFilteredCell();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (defName.Contains("Gas"))
                            {
                                
                                if (building.Def.InputConduitType == ConduitType.Gas)
                                {
                                    loginInCellNum = building.GetUtilityInputCell();
                                }
                                if (building.Def.OutputConduitType == ConduitType.Gas)
                                {
                                    logicOutCellNum = building.GetUtilityOutputCell();
                                }
                                QuantumElementFilter qElemFiltComp = building.GetComponent<QuantumElementFilter>();
                                if (qElemFiltComp != null)
                                {
                                    DebugUtil.Assert(qElemFiltComp.outPortInfo != null, "elementFilter.portInfo was null B");
                                    if (qElemFiltComp.outPortInfo.conduitType == ConduitType.Gas)
                                    {
                                        elemFiltOutCellNum = qElemFiltComp.GetFilteredCell();
                                    }
                                }
                                else
                                {
                                    ElementFilter elemFiltComp = building.GetComponent<ElementFilter>();
                                    if (elemFiltComp != null)
                                    {
                                        DebugUtil.Assert(elemFiltComp.portInfo != null, "elementFilter.portInfo was null B");
                                        if (elemFiltComp.portInfo.conduitType == ConduitType.Gas)
                                        {
                                            elemFiltOutCellNum = elemFiltComp.GetFilteredCell();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (cell == loginInCellNum || cell == logicOutCellNum || cell == elemFiltOutCellNum)
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
                    }
                }
                outBcv = null;
                __result = false;
            }
            return false;
        }
    }
}

