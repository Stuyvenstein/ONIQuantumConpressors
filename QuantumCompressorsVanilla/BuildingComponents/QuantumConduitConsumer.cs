using KSerialization;
using STRINGS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QuantumCompressors.BuildingComponents
{
    [SerializationConfig(MemberSerialization.OptIn)]
    [AddComponentMenu("KMonoBehaviour/scripts/"+nameof(QuantumConduitConsumer))]
    public class QuantumConduitConsumer : KMonoBehaviour //, IConduitConsumer
    {
        #region vars
        [SerializeField]
        public ConduitType conduitType;
        //[MyCmpGet]
        [SerializeField]
        public Storage storage;
        [SerializeField]
        public bool keepZeroMassObject = true;
        [MyCmpReq]
        public Operational operational;
        //[SerializeField]
        //public bool alwaysConsume = false;
        [SerializeField]
        public bool isStorageOn = false;
        [NonSerialized]
        public bool consumedLastTick = true;
        [SerializeField]
        public bool forceAlwaysSatisfied = false;
        [NonSerialized]
        public bool isConsuming = true;
        [SerializeField]
        public Tag capacityTag = GameTags.Any;
        private float capacityKG = float.PositiveInfinity;
        [SerializeField]
        public bool ignoreMinMassCheck = false;
        [SerializeField]
        public bool useSecondaryInput = false;
        [MyCmpReq]
        private Building building;
        private static readonly Operational.Flag storageExistsFlag = new Operational.Flag("storage_avail", Operational.Flag.Type.Requirement);
        public ConduitType ConduitType { get { return conduitType; } }
        public void SetConduitData(ConduitType type) { conduitType = type; }
        public ConduitType TypeOfConduit { get { return conduitType; } }
        public float ConsumptionRate { get { return consumptionRate; } }
        private void OnConduitConnectionChanged(object data) { base.Trigger(-2094018600, IsConnected); }
        public void SetStorageOnState(bool onState) { isStorageOn = onState; }
        private int utilityCell = -1;
        public float consumptionRate = float.PositiveInfinity;
        private bool satisfied = false;
        private HandleVector<int>.Handle partitionerEntry;
        public SimHashes lastConsumedElement = SimHashes.Vacuum;
        public ConduitConsumer.WrongElementResult wrongElementResult = ConduitConsumer.WrongElementResult.Destroy;
        private bool remoteStorageActive = false;
        #endregion
        #region funcs
        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            //TryUpdateStorage();
        }
        protected override void OnSpawn()
        {
            base.OnSpawn();
            TryUpdateStorage();
            GameScheduler.Instance.Schedule("PlumbingTutorial", 2f, delegate (object obj)
            {
                Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_Plumbing, true);
            }, null, null);
            utilityCell = GetInputCell();
            ScenePartitionerLayer layer = GameScenePartitioner.Instance.objectLayers[(conduitType == ConduitType.Gas) ? 12 : 16];
            partitionerEntry = GameScenePartitioner.Instance.Add("QuantumConduitConsumer.OnSpawn", base.gameObject, utilityCell, layer, new Action<object>(OnConduitConnectionChanged));
            GetConduitManager().AddConduitUpdater(new Action<float>(ConduitUpdate), ConduitFlowPriority.Default);
            OnConduitConnectionChanged(null);
        }
        private void ConduitUpdate(float dt)
        {
            TryUpdateStorage();
            if (storage == null && isStorageOn) SetStorageOnState(false);
            else if (storage != null && !isStorageOn) SetStorageOnState(true);
            operational.SetFlag(storageExistsFlag, isStorageOn && remoteStorageActive);
            if (isConsuming)
            {
                ConduitFlow conduitManager = GetConduitManager();
                Consume(dt, conduitManager);
            }
        }
        void TryUpdateStorage(bool force = false)
        {
            var qStorGasComp = GameObject.Find("QuantumStorage" + Enum.GetName(typeof(ConduitType), conduitType));
            if (qStorGasComp != null)
            {
                var operational = qStorGasComp.GetComponent<Operational>();
                if (operational != null)
                {
                    remoteStorageActive = operational.IsOperational;
                }
                if (storage != null && !force) return;
                var storageComp = qStorGasComp.GetComponent<Storage>();
                if (storageComp != null)
                {
                    storage = storageComp;
                    capacityKG = storage.capacityKg;
                }
            }
            //if (storage == null) Debug.Log("Storage not found for "+nameof(QuantumConduitConsumer));
        }
        protected override void OnCleanUp()
        {
            GetConduitManager().RemoveConduitUpdater(new Action<float>(ConduitUpdate));
            GameScenePartitioner.Instance.Free(ref partitionerEntry);
            base.OnCleanUp();
        }
        public Storage Storage
        {
            get
            {
                return storage;
            }
        }
        //private Storage FetchRemoteStorage()
        //{
        //    Storage result = null;
        //    switch (conduitType)
        //    {
        //        case ConduitType.Gas:
        //            result= storageSingleton.gasStorage;
        //            break;
        //        case ConduitType.Liquid:
        //            result = storageSingleton.liquidStorage;
        //            break;
        //    }
        //    return result;
        //}
        //public bool RemoteStorageAvailable()
        //{
        //    bool result = false;
        //    switch (conduitType)
        //    {
        //        case ConduitType.Gas:
        //            result = storageSingleton.gasStorage != null;
        //            break;
        //        case ConduitType.Liquid:
        //            result = storageSingleton.liquidStorage != null;
        //            break;
        //    }
        //    return result;
        //}
        #endregion
        public bool CanConsume //something in pipe
        {
            get
            {
                bool result = false;
                if (IsConnected)
                {
                    ConduitFlow conduitManager = GetConduitManager();
                    result = (conduitManager.GetContents(utilityCell).mass > 0f);
                }
                return result;
            }
        }
        private int GetInputCell() //get pipe input
        {
            int result;
            if (useSecondaryInput)
            {
                ISecondaryInput component = base.GetComponent<ISecondaryInput>();
                result = Grid.OffsetCell(building.NaturalBuildingCell(), component.GetSecondaryConduitOffset()); //conduitType
            }
            else
            {
                result = building.GetUtilityInputCell();
            }
            return result;
        }
        private ConduitFlow GetConduitManager()
        {
            ConduitType conduitType = this.conduitType;
            ConduitFlow result;
            if (conduitType != ConduitType.Gas)
            {
                if (conduitType != ConduitType.Liquid)
                {
                    result = null;
                }
                else
                {
                    result = Game.Instance.liquidConduitFlow;
                }
            }
            else
            {
                result = Game.Instance.gasConduitFlow;
            }
            return result;
        }

        public float MassAvailable
        {
            get
            {
                int inputCell = GetInputCell();
                ConduitFlow conduitManager = GetConduitManager();
                return conduitManager.GetContents(inputCell).mass;
            }
        }
        public bool IsConnected
        {
            get
            {
                GameObject gameObject = Grid.Objects[utilityCell, (conduitType == ConduitType.Gas) ? 12 : 16];
                return gameObject != null && gameObject.GetComponent<BuildingComplete>() != null;
            }
        }
        public bool IsAlmostEmpty
        {
            get
            {
                return !ignoreMinMassCheck && MassAvailable < ConsumptionRate * 30f;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return !ignoreMinMassCheck && (MassAvailable == 0f || MassAvailable < ConsumptionRate);
            }
        }

        public bool IsSatisfied
        {
            get
            {
                return satisfied || !isConsuming;
            }
            set
            {
                satisfied = (value || forceAlwaysSatisfied);
            }
        }
        private void Consume(float dt, ConduitFlow conduit_mgr)
        {
            IsSatisfied = false;
            consumedLastTick = true;
            if (building.Def.CanMove)
            {
                utilityCell = GetInputCell();
            }
            if (IsConnected&&operational.IsOperational)
            {
                ConduitFlow.ConduitContents contents = conduit_mgr.GetContents(utilityCell);
                if (contents.mass > 0)
                {
                    IsSatisfied = true;
                    float num = ConsumptionRate * dt;
                    num = Mathf.Min(num, space_remaining_kg);
                    Element element = ElementLoader.FindElementByHash(contents.element);
                    //if (contents.element != lastConsumedElement)
                    //{
                    //    DiscoveredResources.Instance.Discover(element.tag, element.materialCategory);
                    //}
                    float num2 = 0f;
                    if (num > 0f)
                    {
                        ConduitFlow.ConduitContents conduitContents = conduit_mgr.RemoveElement(utilityCell, num);
                        num2 = conduitContents.mass;
                        lastConsumedElement = conduitContents.element;
                    }
                    if (num2 > 0f && capacityTag != GameTags.Any && !element.HasTag(capacityTag))
                    {
                        base.Trigger(-794517298, new BuildingHP.DamageSourceInfo
                        {
                            damage = 1,
                            source = BUILDINGS.DAMAGESOURCES.BAD_INPUT_ELEMENT,
                            popString = UI.GAMEOBJECTEFFECTS.DAMAGE_POPS.WRONG_ELEMENT
                        });
                    }

                    if (element.HasTag(capacityTag) || wrongElementResult == ConduitConsumer.WrongElementResult.Store || contents.element == SimHashes.Vacuum || capacityTag == GameTags.Any)
                    {
                        if (num2 > 0f)
                        {
                            consumedLastTick = false;
                            int disease_count = (int)((float)contents.diseaseCount * (num2 / contents.mass));
                            Element element2 = ElementLoader.FindElementByHash(contents.element);
                            ConduitType conduitType = this.conduitType;
                            if (conduitType != ConduitType.Gas)
                            {
                                if (conduitType == ConduitType.Liquid)
                                {
                                    if (element2.IsLiquid)
                                    {
                                        storage.AddLiquid(contents.element, num2, contents.temperature, contents.diseaseIdx, disease_count, keepZeroMassObject, false);
                                    }
                                    else
                                    {
                                        global::Debug.LogWarning("Liquid conduit consumer consuming non liquid: " + element2.id.ToString());
                                    }
                                }
                            }
                            else
                            {
                                if (element2.IsGas)
                                {
                                    storage.AddGasChunk(contents.element, num2, contents.temperature, contents.diseaseIdx, disease_count, keepZeroMassObject, false);
                                }
                                else
                                {
                                    global::Debug.LogWarning("Gas conduit consumer consuming non gas: " + element2.id.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        if (num2 > 0f)
                        {
                            consumedLastTick = false;
                            if (wrongElementResult == ConduitConsumer.WrongElementResult.Dump)
                            {
                                int disease_count2 = (int)((float)contents.diseaseCount * (num2 / contents.mass));
                                int gameCell = Grid.PosToCell(base.transform.GetPosition());
                                SimMessages.AddRemoveSubstance(gameCell, contents.element, CellEventLogger.Instance.ConduitConsumerWrongElement, num2, contents.temperature, contents.diseaseIdx, disease_count2, true, -1);
                            }
                        }
                    }
                }
            }
        }
        public float stored_mass
        {
            get
            {
                return (storage == null) ? 0f : ((capacityTag != GameTags.Any) ? storage.GetMassAvailable(capacityTag) : storage.MassStored());
            }
        }
        public float space_remaining_kg
        {
            get
            {
                float num = capacityKG - stored_mass;
                return (storage == null) ? num : Mathf.Min(storage.RemainingCapacity(), num);
            }
        }
    }

}
