using KSerialization;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QuantumCompressors.BuildingComponents
{
    [SerializationConfig(MemberSerialization.OptIn)]
    [AddComponentMenu("KMonoBehaviour/scripts/"+ nameof(QuantumElementFilter))]
    public class QuantumElementFilter :KMonoBehaviour, ISaveLoadable //, ISecondaryOutput
    {
        [SerializeField]
        public ConduitPortInfo outPortInfo;
        [SerializeField]
        public ConduitPortInfo inPortInfo;
        [MyCmpReq]
        private Operational operational;
        [MyCmpReq]
        private Building building;
        [MyCmpReq]
        private KSelectable selectable;
        [MyCmpReq]
        private Filterable filterable;
        //[NonSerialized]
        private bool isConsuming = true;
        //[NonSerialized]
        private bool consumedLastTick = true;
        //[SerializeField]
        //private bool forceAlwaysSatisfied = false;
        [SerializeField]
        public bool outBlocked = true;
        [SerializeField]
        public bool outEmpty = true;
        [SerializeField]
        public bool keepZeroMassObject = true;
        [SerializeField]
        public bool useInput = false;
        //[SerializeField]
        //public bool alwaysDispense = false;
        [SerializeField]
        public float capacityKG = float.PositiveInfinity;
        [SerializeField]
        public bool storageOnState = false;
        [SerializeField]
        public Tag capacityTag = GameTags.Any;
        [SerializeField]
        public Storage storage;
        public SimHashes lastConsumedElement = SimHashes.Vacuum;
        private Guid needsConduitStatusItemGuid;
        private bool satisfied = false;
        private Guid conduitBlockedStatusItemGuid;
        private HandleVector<int>.Handle partitionerEntryOut;
        private HandleVector<int>.Handle partitionerEntryIn;
        private int filteredCell = -1;
        private int inputCell = -1;
        public float consumptionRate = float.PositiveInfinity;
        private static StatusItem filterStatusItem = null;
        public ConduitConsumer.WrongElementResult wrongElementResult = ConduitConsumer.WrongElementResult.Destroy;
        private int elementOutputOffset = 0;

        private static readonly Operational.Flag storageExistsFlag = new Operational.Flag("storage_avail", Operational.Flag.Type.Requirement);

        SolidConduitFlow solidFlowManager{get{return SolidConduit.GetFlowManager();}}
        ConduitFlow fluidFlowManager{get{return Conduit.GetFlowManager(outPortInfo.conduitType);}}
        public void SetStorageOnState(bool onState){storageOnState = onState;}
        //public Storage Storage { get { return storage; } }
        //public ConduitType ConduitType { get { return outPortInfo.conduitType; } }
        private bool qStorageOperational = false;
        public int GetFilteredCell()
        {
            return filteredCell;
        }
        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            InitializeStatusItems();
            //TryUpdateStorage();
        }
        void TryUpdateStorage(bool force=false)
        {
            BuildingComplete com = GetComponent<BuildingComplete>();
            QuantumStorageSingleton storageSingleton = QuantumStorageSingleton.Get();
            var storageItem = storageSingleton.StorageItems.Where(i => (i.conduitType == outPortInfo.conduitType && i.worldId == com.GetMyWorldId())).FirstOrDefault();
            if (storageItem != null)
            {
                qStorageOperational = storageItem.operational.IsOperational;
                storage = storageItem.storage;
                capacityKG = storageItem.storage.capacityKg;
            }

        }
        protected override void OnSpawn()
        {
            base.OnSpawn();
            TryUpdateStorage();
            ScenePartitionerLayer partitionerLayer = null;
            if (outPortInfo.conduitType == ConduitType.Solid)
            {
                solidFlowManager.AddConduitUpdater(new Action<float>(OnConduitTick), ConduitFlowPriority.Default);
                partitionerLayer = GameScenePartitioner.Instance.objectLayers[(int)ObjectLayer.SolidConduit];
            }
            else
            {
                fluidFlowManager.AddConduitUpdater(new Action<float>(OnConduitTick), ConduitFlowPriority.Default);
                switch (outPortInfo.conduitType)
                {
                    case ConduitType.Gas:
                        partitionerLayer = GameScenePartitioner.Instance.objectLayers[(int)ObjectLayer.GasConduit];
                        break;
                    case ConduitType.Liquid:
                        partitionerLayer = GameScenePartitioner.Instance.objectLayers[(int)ObjectLayer.LiquidConduit];
                        break;
                }
            }
            filteredCell = building.GetUtilityOutputCell();
            if (partitionerLayer != null)
            {
                partitionerEntryOut = GameScenePartitioner.Instance.Add("QElementFilterConduitExists", gameObject, filteredCell, partitionerLayer, new Action<object>(OnOutConduitConnectionChanged));
            }
            if (useInput)
            {
                inputCell = building.GetUtilityInputCell();
                if (partitionerLayer != null)
                {
                    partitionerEntryIn = GameScenePartitioner.Instance.Add("QElementInConduitExists", gameObject, inputCell, partitionerLayer, new Action<object>(OnInConduitConnectionChanged));
                }

            }

            OnFilterChanged(filterable.SelectedTag);
            filterable.onFilterChanged += new Action<Tag>(OnFilterChanged);
            GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, filterStatusItem, this);
            UpdateOutConduitBlockedStatus();
            OnOutConduitConnectionChanged(null);
            OnInConduitConnectionChanged(null);
        }
        private void OnInConduitConnectionChanged(object data)
        {
            Trigger((int)GameHashes.ConduitConnectionChanged, IsInConnected);
        }
        private void OnOutConduitConnectionChanged(object data)
        {
            Trigger((int)GameHashes.ConduitConnectionChanged, IsOutConnected);
        }
        private void OnConduitTick(float dt)
        {
            
            TryUpdateStorage();
            outBlocked = false;
            UpdateOutConduitBlockedStatus();
            if (storage == null && storageOnState) SetStorageOnState(false);
            else if (storage != null && !storageOnState) SetStorageOnState(true);
            operational.SetFlag(storageExistsFlag, storageOnState && qStorageOperational);
            if (operational.IsOperational)
            {
                if (IsOutConnected)
                {
                    switch (outPortInfo.conduitType)
                    {
                        case ConduitType.Gas:
                        case ConduitType.Liquid:
                            ConduitFlow flowManager = fluidFlowManager;
                            PrimaryElement primaryElement = FindSuitableElement();
                            if (primaryElement != null)
                            {
                                outEmpty = false;
                                float flow = flowManager.AddElement(filteredCell, primaryElement.ElementID, primaryElement.Mass, primaryElement.Temperature, primaryElement.DiseaseIdx, primaryElement.DiseaseCount);
                                if (flow <= 0f) outBlocked = true;
                                else
                                {
                                    float chunk = flow / primaryElement.Mass;
                                    int chunkDisease = (int)(chunk * (float)primaryElement.DiseaseCount);
                                    primaryElement.ModifyDiseaseCount(-chunkDisease, "QuantumElementFilter.ConduitUpdate");
                                    primaryElement.Mass -= flow;
                                    base.Trigger((int)GameHashes.OnStorageChange, primaryElement.gameObject);
                                }
                            }
                            else
                            {
                                outEmpty = true;
                            }
                            break;
                        case ConduitType.Solid:

                            break;
                    }
                }
                if (useInput)
                {
                    if (IsInConnected)
                    {
                        ConduitFlow flowManager = fluidFlowManager;
                        IsSatisfied = false;
                        consumedLastTick = true;
                        ConduitFlow.ConduitContents contents = flowManager.GetContents(inputCell);
                        if (contents.mass > 0f)
                        {
                            IsSatisfied = true;
                            float consume = consumptionRate * dt;
                            consume = Mathf.Min(consume, space_remaining_kg);
                            Element element = ElementLoader.FindElementByHash(contents.element);
                            if (contents.element != lastConsumedElement)
                            {
                                DiscoveredResources.Instance.Discover(element.tag, element.materialCategory);
                            }
                            float contentMass = 0f;
                            if (consume > 0f)
                            {
                                ConduitFlow.ConduitContents conduitContents = flowManager.RemoveElement(inputCell, consume);
                                contentMass = conduitContents.mass;
                                lastConsumedElement = conduitContents.element;
                            }
                            bool elementHasCapTag = element.HasTag(capacityTag);
                            if (contentMass > 0f && capacityTag != GameTags.Any && !elementHasCapTag)
                            {
                                base.Trigger((int)GameHashes.DoBuildingDamage, new BuildingHP.DamageSourceInfo
                                {
                                    damage = 1,
                                    source = BUILDINGS.DAMAGESOURCES.BAD_INPUT_ELEMENT,
                                    popString = UI.GAMEOBJECTEFFECTS.DAMAGE_POPS.WRONG_ELEMENT
                                });
                            }
                            if (elementHasCapTag || wrongElementResult == ConduitConsumer.WrongElementResult.Store || contents.element == SimHashes.Vacuum || capacityTag == GameTags.Any)
                            {
                                if (contentMass > 0f)
                                {
                                    consumedLastTick = false;
                                    int disease_count = (int)((float)contents.diseaseCount * (contentMass / contents.mass));
                                    Element element2 = ElementLoader.FindElementByHash(contents.element);
                                    ConduitType conduitType = inPortInfo.conduitType;
                                    if (conduitType != ConduitType.Gas)
                                    {
                                        if (conduitType == ConduitType.Liquid)
                                        {
                                            if (element2.IsLiquid)
                                            {
                                                storage.AddLiquid(contents.element, contentMass, contents.temperature, contents.diseaseIdx, disease_count, keepZeroMassObject, false);
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
                                            storage.AddGasChunk(contents.element, contentMass, contents.temperature, contents.diseaseIdx, disease_count, keepZeroMassObject, false);
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
                                if (contentMass > 0f)
                                {
                                    consumedLastTick = false;
                                    bool flag11 = wrongElementResult == ConduitConsumer.WrongElementResult.Dump;
                                    if (flag11)
                                    {
                                        int disease_count2 = (int)((float)contents.diseaseCount * (contentMass / contents.mass));
                                        int gameCell = Grid.PosToCell(base.transform.GetPosition());
                                        SimMessages.AddRemoveSubstance(gameCell, contents.element, CellEventLogger.Instance.ConduitConsumerWrongElement, contentMass, contents.temperature, contents.diseaseIdx, disease_count2, true, -1);
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }
        public float space_remaining_kg
        {
            get
            {
                float remainingStorage = capacityKG - stored_mass;
                return (storage == null) ? remainingStorage : Mathf.Min(storage.RemainingCapacity(), remainingStorage);
            }
        }
        public float stored_mass
        {
            get
            {
                return (storage == null) ? 0f : ((capacityTag != GameTags.Any) ? storage.GetMassAvailable(capacityTag) : storage.MassStored());
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
                satisfied = value;
            }
        }
        private PrimaryElement FindSuitableElement()
        {
            List<GameObject> allItems = storage.items;//.Where(i=>i.tag.Equals(filterable.SelectedTag)).ToList();
            List<GameObject> filterItems = new List<GameObject>();
            foreach (var item in allItems)
            {
                if(item.GetComponent<PrimaryElement>().ElementID.CreateTag()== filterable.SelectedTag)
                {
                    filterItems.Add(item);
                }
            }
            for (int i = 0; i < filterItems.Count; i++)
            {
                int flow = (i + elementOutputOffset) % filterItems.Count;
                PrimaryElement component = filterItems[flow].GetComponent<PrimaryElement>();
                bool isTransferrable = false;
                switch (outPortInfo.conduitType)
                {
                    case ConduitType.Gas:
                        isTransferrable = component.Element.IsGas;
                        break;
                    case ConduitType.Liquid:
                        isTransferrable = component.Element.IsLiquid;
                        break;
                    case ConduitType.Solid:
                        isTransferrable = component.Element.IsSolid;
                        break;
                }
                if (component != null && component.Mass > 0f && isTransferrable)
                {
                    elementOutputOffset = (elementOutputOffset + 1) % filterItems.Count;
                    return component;
                }
            }
            return null;
        }
        protected override void OnCleanUp()
        {
            switch (outPortInfo.conduitType)
            {
                case ConduitType.Gas:
                case ConduitType.Liquid:
                    fluidFlowManager.RemoveConduitUpdater(new Action<float>(OnConduitTick));
                    break;
                case ConduitType.Solid:
                    solidFlowManager.RemoveConduitUpdater(new Action<float>(OnConduitTick));
                    break;
            }
            if (partitionerEntryOut.IsValid() && GameScenePartitioner.Instance != null)
            {
                GameScenePartitioner.Instance.Free(ref partitionerEntryOut);
            }
            if (useInput)
            {
                if (partitionerEntryIn.IsValid() && GameScenePartitioner.Instance != null)
                {
                    GameScenePartitioner.Instance.Free(ref partitionerEntryIn);
                }
            }
            
            base.OnCleanUp();
        }
        public bool IsInConnected
        {
            get
            {
                if (useInput)
                {
                    GameObject gameObject = null;
                    switch (outPortInfo.conduitType)
                    {
                        case ConduitType.Gas:
                            gameObject = Grid.Objects[inputCell, (int)ObjectLayer.GasConduit];
                            break;
                        case ConduitType.Liquid:
                            gameObject = Grid.Objects[inputCell, (int)ObjectLayer.LiquidConduit];
                            break;
                        case ConduitType.Solid:
                            gameObject = Grid.Objects[inputCell, (int)ObjectLayer.SolidConduit];
                            break;
                    }
                    return (gameObject != null && gameObject.GetComponent<BuildingComplete>() != null);
                }
                return false;
            }
        }
        public bool IsOutConnected
        {
            get
            {
                GameObject gameObject = null;
                switch (outPortInfo.conduitType)
                {
                    case ConduitType.Gas:
                        gameObject = Grid.Objects[filteredCell, (int)ObjectLayer.GasConduit];
                        break;
                    case ConduitType.Liquid:
                        gameObject = Grid.Objects[filteredCell, (int)ObjectLayer.LiquidConduit];
                        break;
                    case ConduitType.Solid:
                        gameObject = Grid.Objects[filteredCell, (int)ObjectLayer.SolidConduit];
                        break;
                }
                return (gameObject != null && gameObject.GetComponent<BuildingComplete>() != null);
            }
        }

        private void UpdateOutConduitBlockedStatus()
        {
            bool isConEmpty = fluidFlowManager.IsConduitEmpty(filteredCell);
            StatusItem conduitBlockedMultiples = Db.Get().BuildingStatusItems.ConduitBlockedMultiples;
            bool statIdSet = conduitBlockedStatusItemGuid != Guid.Empty;
            if (isConEmpty == statIdSet)
            {
                conduitBlockedStatusItemGuid = selectable.ToggleStatusItem(conduitBlockedMultiples, conduitBlockedStatusItemGuid, !isConEmpty, null);
            }
        }
        private void OnFilterChanged(Tag tag)
        {
            bool on = !tag.IsValid || tag == GameTags.Void;
            GetComponent<KSelectable>().ToggleStatusItem(Db.Get().BuildingStatusItems.NoFilterElementSelected, on, null);
        }

        private void InitializeStatusItems()
        {
            if (filterStatusItem == null)
            {
				filterStatusItem = new StatusItem("Filter", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.LiquidConduits.ID, true, 129022, null);
				filterStatusItem.resolveStringCallback = delegate (string str, object data)
                {
                    QuantumElementFilter elementFilter = (QuantumElementFilter)data;
                    if (!elementFilter.filterable.SelectedTag.IsValid || elementFilter.filterable.SelectedTag == GameTags.Void)
                    {
                        str = string.Format(BUILDINGS.PREFABS.GASFILTER.STATUS_ITEM, BUILDINGS.PREFABS.GASFILTER.ELEMENT_NOT_SPECIFIED);
                    }
                    else
                    {
                        str = string.Format(BUILDINGS.PREFABS.GASFILTER.STATUS_ITEM, elementFilter.filterable.SelectedTag.ProperName());
                    }
                    return str;
                };
				filterStatusItem.conditionalOverlayCallback = new Func<HashedString, object, bool>(ShowInUtilityOverlay);
            }
        }

        private bool ShowInUtilityOverlay(HashedString mode, object data)
        {
            bool result = false;
            QuantumElementFilter elementFilter = (QuantumElementFilter)data;
            switch (elementFilter.outPortInfo.conduitType)
            {
                case ConduitType.Gas:
                    result = (mode == OverlayModes.GasConduits.ID);
                    break;
                case ConduitType.Liquid:
                    result = (mode == OverlayModes.LiquidConduits.ID);
                    break;
                case ConduitType.Solid:
                    result = (mode == OverlayModes.SolidConveyor.ID);
                    break;
            }
            return result;
        }
        
    }
}
