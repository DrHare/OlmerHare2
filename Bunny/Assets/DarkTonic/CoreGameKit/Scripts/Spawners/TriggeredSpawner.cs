using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_4_6 || UNITY_5_0
	using UnityEngine.UI;
	using UnityEngine.EventSystems;
#endif

/// <summary>
/// This class is used for Triggered Spawner setup.
/// </summary>
[AddComponentMenu("Dark Tonic/Core GameKit/Spawners/Triggered Spawner")]
public class TriggeredSpawner : MonoBehaviour, ICgkEventReceiver 
		#if UNITY_4_6 || UNITY_5_0
        , IPointerDownHandler
		, IDragHandler
		, IPointerUpHandler
		, IPointerEnterHandler
		, IPointerExitHandler
		, IDropHandler
		, IScrollHandler
		, IUpdateSelectedHandler
		, ISelectHandler, IDeselectHandler, IMoveHandler, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, ISubmitHandler, ICancelHandler
		#endif
{
    public const int MAX_DISTANCE = 5000;

    #if UNITY_4_6 || UNITY_5_0
        public Unity_UIVersion unityUIMode = Unity_UIVersion.uGUI;
	#else
		public Unity_UIVersion unityUIMode = Unity_UIVersion.Legacy;
	#endif

	public enum Unity_UIVersion {
		Legacy
#if UNITY_4_6 || UNITY_5_0
         ,uGUI
#endif
	}

    #region categorizations of event types
    public static List<TriggeredSpawner.EventType> eventsThatCanRepeatWave = new List<TriggeredSpawner.EventType>() {
		EventType.OnEnabled,
		EventType.Visible,
		EventType.OnTriggerEnter,
		EventType.OnTriggerExit,
		EventType.MouseClick_Legacy,
		EventType.MouseOver_Legacy,
		EventType.OnCollision,
		EventType.OnSpawned,
		EventType.CodeTriggered1,
		EventType.CodeTriggered2,
		EventType.OnClick_NGUI,
		EventType.OnCollision2D,
		EventType.OnTriggerEnter2D,
		EventType.OnTriggerExit2D,
		EventType.CustomEvent,
		EventType.SliderChanged_uGUI,
		EventType.ButtonClicked_uGUI,
		EventType.PointerDown_uGUI,
		EventType.PointerUp_uGUI,
		EventType.PointerEnter_uGUI,
		EventType.PointerExit_uGUI,
		EventType.Drag_uGUI,
		EventType.Drop_uGUI,
		EventType.Scroll_uGUI,
		EventType.UpdateSelected_uGUI,
		EventType.Select_uGUI,
		EventType.Deselect_uGUI,
		EventType.Move_uGUI,
		EventType.InitializePotentialDrag_uGUI,
		EventType.BeginDrag_uGUI,
		EventType.EndDrag_uGUI,
		EventType.Submit_uGUI,
		EventType.Cancel_uGUI
	};

    public static List<TriggeredSpawner.EventType> eventsWithTagLayerFilters = new List<TriggeredSpawner.EventType>() {
		EventType.OnCollision,
		EventType.OnTriggerEnter,
		EventType.OnTriggerExit,
		EventType.OnCollision2D,
		EventType.OnTriggerEnter2D,
		EventType.OnTriggerExit2D
	};

    public static List<TriggeredSpawner.EventType> eventsWithInflexibleWaveLength = new List<TriggeredSpawner.EventType>() {
		EventType.Invisible,
		EventType.OnDespawned,
		EventType.OnDisabled
	};

    public static List<TriggeredSpawner.EventType> eventsThatCanTriggerDespawn = new List<TriggeredSpawner.EventType>() {
		EventType.MouseClick_Legacy,
		EventType.MouseOver_Legacy,
		EventType.OnCollision,
		EventType.OnTriggerEnter,
		EventType.OnTriggerExit,
		EventType.OnCollision2D,
		EventType.OnTriggerEnter2D,
		EventType.OnTriggerExit2D,
		EventType.Visible,
		EventType.OnEnabled,
		EventType.OnSpawned,
		EventType.CodeTriggered1,
		EventType.CodeTriggered2,
		EventType.OnClick_NGUI,
		EventType.CustomEvent,
		EventType.SliderChanged_uGUI,
		EventType.ButtonClicked_uGUI,
		EventType.PointerDown_uGUI,
		EventType.PointerUp_uGUI,
		EventType.PointerEnter_uGUI,
		EventType.PointerExit_uGUI,
		EventType.Drag_uGUI,
		EventType.Drop_uGUI,
		EventType.Scroll_uGUI,
		EventType.UpdateSelected_uGUI,
		EventType.Select_uGUI,
		EventType.Deselect_uGUI,
		EventType.Move_uGUI,
		EventType.InitializePotentialDrag_uGUI,
		EventType.BeginDrag_uGUI,
		EventType.EndDrag_uGUI,
		EventType.Submit_uGUI,
		EventType.Cancel_uGUI
	};
    #endregion

	public bool logMissingEvents = true;
	public LevelSettings.ActiveItemMode activeMode = LevelSettings.ActiveItemMode.Always;
    public WorldVariableRangeCollection activeItemCriteria = new WorldVariableRangeCollection();

    public GameOverBehavior gameOverBehavior = GameOverBehavior.Disable;
    public WavePauseBehavior wavePauseBehavior = WavePauseBehavior.Disable;
    public SpawnerEventSource eventSourceType = SpawnerEventSource.Self;
    public bool transmitEventsToChildren = true;

    public WaveSyncroPrefabSpawner.SpawnLayerTagMode spawnLayerMode = WaveSyncroPrefabSpawner.SpawnLayerTagMode.UseSpawnPrefabSettings;
    public WaveSyncroPrefabSpawner.SpawnLayerTagMode spawnTagMode = WaveSyncroPrefabSpawner.SpawnLayerTagMode.UseSpawnPrefabSettings;
    public int spawnCustomLayer = 0;
    public string spawnCustomTag = "Untagged";

    public TriggeredSpawnerListener listener = null;

    public TriggeredWaveSpecifics enableWave = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics disableWave = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics visibleWave = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics invisibleWave = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics mouseOverWave = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics mouseClickWave = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics collisionWave = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics triggerEnterWave = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics triggerExitWave = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics spawnedWave = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics despawnedWave = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics codeTriggeredWave1 = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics codeTriggeredWave2 = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics clickWave = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics collision2dWave = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics triggerEnter2dWave = new TriggeredWaveSpecifics();
    public TriggeredWaveSpecifics triggerExit2dWave = new TriggeredWaveSpecifics();
	public List<TriggeredWaveSpecifics> userDefinedEventWaves = new List<TriggeredWaveSpecifics>();
	public TriggeredWaveSpecifics unitySliderChangedWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityButtonClickedWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityPointerDownWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityPointerUpWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityPointerEnterWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityPointerExitWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityDragWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityDropWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityScrollWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityUpdateSelectedWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unitySelectWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityDeselectWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityMoveWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityInitializePotentialDragWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityBeginDragWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityEndDragWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unitySubmitWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics unityCancelWave = new TriggeredWaveSpecifics();

		
	private TriggeredWaveMetaData enableWaveMeta = null;
    private TriggeredWaveMetaData disableWaveMeta = null;
    private TriggeredWaveMetaData visibleWaveMeta = null;
    private TriggeredWaveMetaData invisibleWaveMeta = null;
    private TriggeredWaveMetaData mouseOverWaveMeta = null;
    private TriggeredWaveMetaData mouseClickWaveMeta = null;
    private TriggeredWaveMetaData collisionWaveMeta = null;
    private TriggeredWaveMetaData triggerEnterWaveMeta = null;
    private TriggeredWaveMetaData triggerExitWaveMeta = null;
    private TriggeredWaveMetaData spawnedWaveMeta = null;
    private TriggeredWaveMetaData despawnedWaveMeta = null;
    private TriggeredWaveMetaData codeTriggeredWave1Meta = null;
    private TriggeredWaveMetaData codeTriggeredWave2Meta = null;
    private TriggeredWaveMetaData clickWaveMeta = null;
    private TriggeredWaveMetaData collision2dWaveMeta = null;
    private TriggeredWaveMetaData triggerEnter2dWaveMeta = null;
    private TriggeredWaveMetaData triggerExit2dWaveMeta = null;
	private List<TriggeredWaveMetaData> userDefinedEventWaveMeta = new List<TriggeredWaveMetaData>();
	private TriggeredWaveMetaData unitySliderChangedWaveMeta = null;
	private TriggeredWaveMetaData unityButtonClickedWaveMeta = null;
	private TriggeredWaveMetaData unityPointerDownWaveMeta = null;
	private TriggeredWaveMetaData unityPointerUpWaveMeta = null;
	private TriggeredWaveMetaData unityPointerEnterWaveMeta = null;
	private TriggeredWaveMetaData unityPointerExitWaveMeta = null;
	private TriggeredWaveMetaData unityDragWaveMeta = null;
	private TriggeredWaveMetaData unityDropWaveMeta = null;
	private TriggeredWaveMetaData unityScrollWaveMeta = null;
	private TriggeredWaveMetaData unityUpdateSelectedWaveMeta = null;
	private TriggeredWaveMetaData unitySelectWaveMeta = null;
	private TriggeredWaveMetaData unityDeselectWaveMeta = null;
	private TriggeredWaveMetaData unityMoveWaveMeta = null;
	private TriggeredWaveMetaData unityInitializePotentialDragWaveMeta = null;
	private TriggeredWaveMetaData unityBeginDragWaveMeta = null;
	private TriggeredWaveMetaData unityEndDragWaveMeta = null;
	private TriggeredWaveMetaData unitySubmitWaveMeta = null;
	private TriggeredWaveMetaData unityCancelWaveMeta = null;

    #if UNITY_4_6 || UNITY_5_0
		private UnityEngine.UI.Button button = null;
		private UnityEngine.UI.Slider slider = null;
	#endif 

	private Transform _trans;
    private GameObject go;
    private bool isVisible;
	private List<TriggeredSpawner> childSpawners = new List<TriggeredSpawner>();
	
    #region Enums
    public enum SpawnerEventSource {
        ReceiveFromParent,
        Self,
        None
    }

    public enum GameOverBehavior {
        BehaveAsNormal,
        Disable
    }

    public enum WavePauseBehavior {
        BehaveAsNormal,
        Disable
    }

    public enum RetriggerLimitMode {
        None,
        FrameBased,
        TimeBased
    }

    public enum EventType {
        OnEnabled,
        OnDisabled,
        Visible,
        Invisible,
        MouseOver_Legacy,
        MouseClick_Legacy,
        OnCollision,
        OnTriggerEnter,
        OnSpawned,
        OnDespawned,
        OnClick_NGUI,
        CodeTriggered1,
        CodeTriggered2,
        LostHitPoints,
        OnTriggerExit,
        OnCollision2D,
        OnTriggerEnter2D, 
        OnTriggerExit2D,
        SpawnerDestroyed,
		DeathTimer,
		CustomEvent,
		SliderChanged_uGUI,
		ButtonClicked_uGUI,
		PointerDown_uGUI,
		PointerUp_uGUI,
		PointerEnter_uGUI,
		PointerExit_uGUI,
		Drag_uGUI,
		Drop_uGUI,
		Scroll_uGUI,
		UpdateSelected_uGUI,
		Select_uGUI,
		Deselect_uGUI,
		Move_uGUI,
		InitializePotentialDrag_uGUI,
		BeginDrag_uGUI,
		EndDrag_uGUI,
		Submit_uGUI,
		Cancel_uGUI
	}
    #endregion

    void Awake() {
        this.go = this.gameObject;

        #if UNITY_4_6 || UNITY_5_0
			button = this.GetComponent<UnityEngine.UI.Button>();
			slider = this.GetComponent<UnityEngine.UI.Slider>();
		#endif

		SpawnedOrAwake();
    }

    void Start() {
        CheckForValidVariablesForWave(enableWave, EventType.OnEnabled);
        CheckForValidVariablesForWave(disableWave, EventType.OnDisabled);
        CheckForValidVariablesForWave(visibleWave, EventType.Visible);
        CheckForValidVariablesForWave(invisibleWave, EventType.Invisible);
        CheckForValidVariablesForWave(mouseOverWave, EventType.MouseOver_Legacy);
        CheckForValidVariablesForWave(mouseClickWave, EventType.MouseClick_Legacy);
        CheckForValidVariablesForWave(collisionWave, EventType.OnCollision);
        CheckForValidVariablesForWave(triggerEnterWave, EventType.OnTriggerEnter);
        CheckForValidVariablesForWave(triggerExitWave, EventType.OnTriggerExit);
        CheckForValidVariablesForWave(spawnedWave, EventType.OnSpawned);
        CheckForValidVariablesForWave(despawnedWave, EventType.OnDespawned);
        CheckForValidVariablesForWave(clickWave, EventType.OnClick_NGUI);
        CheckForValidVariablesForWave(codeTriggeredWave1, EventType.CodeTriggered1);
        CheckForValidVariablesForWave(codeTriggeredWave2, EventType.CodeTriggered2);
        CheckForValidVariablesForWave(collision2dWave, EventType.OnCollision2D);
        CheckForValidVariablesForWave(triggerEnter2dWave, EventType.OnTriggerEnter2D);
        CheckForValidVariablesForWave(triggerExit2dWave, EventType.OnTriggerExit2D);

#if UNITY_4_6 || UNITY_5_0
        CheckForValidVariablesForWave(unitySliderChangedWave, EventType.SliderChanged_uGUI);
		CheckForValidVariablesForWave(unityButtonClickedWave, EventType.ButtonClicked_uGUI);
		CheckForValidVariablesForWave(unityPointerDownWave, EventType.PointerDown_uGUI);
		CheckForValidVariablesForWave(unityPointerUpWave, EventType.PointerUp_uGUI);
		CheckForValidVariablesForWave(unityPointerEnterWave, EventType.PointerEnter_uGUI);
#endif

        // check active item mode
        if (activeMode == LevelSettings.ActiveItemMode.IfWorldVariableInRange || activeMode == LevelSettings.ActiveItemMode.IfWorldVariableOutsideRange) {
            for (var i = 0; i < activeItemCriteria.statMods.Count; i++) {
                var crit = activeItemCriteria.statMods[i];

                if (WorldVariableTracker.IsBlankVariableName(crit._statName)) {
                    LevelSettings.LogIfNew(string.Format("Spawner '{0}' has an Active Item Limit criteria with no World Variable selected. Please select one.",
                        Trans.name));
                } else if (!WorldVariableTracker.VariableExistsInScene(crit._statName)) {
                    LevelSettings.LogIfNew(string.Format("Spawner '{0}' has an Active Item Limit criteria criteria of World Variable '{1}', which doesn't exist in the scene.",
                        Trans.name,
                        crit._statName));
                }
            }
        }

		CheckForIllegalCustomEvents();
    }

    #region Propogate Events
    private void PropagateEventToChildSpawners(EventType eType) {
        if (!transmitEventsToChildren) {
            return;
        }

        if (childSpawners.Count > 0) {
            if (listener != null) {
                listener.EventPropagating(eType, Trans, childSpawners.Count);
            }

            for (var i = 0; i < childSpawners.Count; i++) {
                childSpawners[i].PropagateEventTrigger(eType, Trans);
            }
        }
    }

    public void PropagateEventTrigger(EventType eType, Transform transmitterTrans) {
        if (listener != null) {
            listener.PropagatedEventReceived(eType, transmitterTrans);
        }

        switch (eType) {
            case EventType.CodeTriggered1:
                ActivateCodeTriggeredEvent1();
                break;
            case EventType.CodeTriggered2:
                ActivateCodeTriggeredEvent2();
                break;
            case EventType.Invisible:
                _OnBecameInvisible(false);
                break;
			case EventType.MouseClick_Legacy:
                _OnMouseDown(false);
                break;
			case EventType.MouseOver_Legacy:
                _OnMouseEnter(false);
                break;
			case EventType.OnClick_NGUI:
                _OnClick(false);
                break;
            case EventType.OnCollision:
                _OnCollisionEnter(false);
                break;
            case EventType.OnDespawned:
                _OnDespawned(false);
                break;
            case EventType.OnDisabled:
                _DisableEvent(false);
                break;
            case EventType.OnEnabled:
                _EnableEvent(false);
                break;
            case EventType.OnSpawned:
                _OnSpawned(false);
                break;
            case EventType.OnTriggerEnter:
                _OnTriggerEnter(false);
                break;
            case EventType.OnTriggerExit:
                _OnTriggerExit(false);
                break;
            case EventType.Visible:
                _OnBecameVisible(false);
                break;
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
            // not supported
#else
				case EventType.OnCollision2D:
					_OnCollision2dEnter(false);
					break;
				case EventType.OnTriggerEnter2D:
					_OnTriggerEnter2D(false);
					break;
				case EventType.OnTriggerExit2D:
					_OnTriggerExit2D(false);
					break;
#endif
#if UNITY_4_6 || UNITY_5_0
            case EventType.SliderChanged_uGUI:
				_SliderChanged(false);
				break;
			case EventType.ButtonClicked_uGUI:
				_ButtonClicked(false);
				break;
			case EventType.PointerDown_uGUI:
				_OnPointerDown(false);
				break;
			case EventType.PointerUp_uGUI:
				_OnPointerUp(false);
				break;
			case EventType.PointerEnter_uGUI:
				_OnPointerEnter(false);
				break;
			case EventType.PointerExit_uGUI:
				_OnPointerExit(false);
				break;
			case EventType.Drag_uGUI:
				_OnDrag(false);
				break;
			case EventType.Drop_uGUI:
				_OnDrop(false);
				break;
			case EventType.Scroll_uGUI:
				_OnScroll(false);
				break;
			case EventType.UpdateSelected_uGUI:
				_OnUpdateSelected(false);
				break;
			case EventType.Select_uGUI:
				_OnSelect(false);
				break;
			case EventType.Deselect_uGUI:
				_OnDeselect(false);
				break;
			case EventType.Move_uGUI:
				_OnMove(false);
				break;
			case EventType.InitializePotentialDrag_uGUI:
				_OnInitializePotentialDrag(false);
				break;
			case EventType.BeginDrag_uGUI:
				_OnBeginDrag(false);
				break;
			case EventType.EndDrag_uGUI:
				_OnEndDrag(false);
				break;
			case EventType.Submit_uGUI:
				_OnSubmit(false);
				break;
			case EventType.Cancel_uGUI:
				_OnCancel(false);
				break;
#endif
        }
    }
    #endregion

    #region CodeTriggeredEvents
    /// <summary>
    /// Call this method to active Code-Triggered Event 1.
    /// </summary>
    public void ActivateCodeTriggeredEvent1() {
        var eType = EventType.CodeTriggered1;

        if (!IsWaveValid(codeTriggeredWave1, eType, false)) {
            return;
        }

        if (SetupNextWave(codeTriggeredWave1, eType)) {
            if (listener != null) {
                listener.WaveStart(eType, codeTriggeredWave1Meta.waveSpec);
            }
        }
        SpawnFromWaveMeta(codeTriggeredWave1Meta, eType);

        PropagateEventToChildSpawners(eType);
    }

    /// <summary>
    /// Call this method to active Code-Triggered Event 2.
    /// </summary>
    public void ActivateCodeTriggeredEvent2() {
        var eType = EventType.CodeTriggered2;

        if (!IsWaveValid(codeTriggeredWave2, eType, false)) {
            return;
        }

        if (SetupNextWave(codeTriggeredWave2, eType)) {
            if (listener != null) {
                listener.WaveStart(eType, codeTriggeredWave2Meta.waveSpec);
            }
        }
        SpawnFromWaveMeta(codeTriggeredWave2Meta, eType);

        PropagateEventToChildSpawners(eType);
    }
    #endregion

    #region OnEnable
    void OnEnable() {
        #if UNITY_4_6 || UNITY_5_0
            if (slider != null) {
			    slider.onValueChanged.AddListener(SliderChanged);
		    }
		    if (button != null) {
			    button.onClick.AddListener(ButtonClicked);	
		    }
		#endif

		
		_EnableEvent(true);
    }

    private void _EnableEvent(bool calledFromSelf) {
        var eType = EventType.OnEnabled;

        RegisterReceiver();

        if (!IsWaveValid(enableWave, eType, calledFromSelf)) {
            return;
        }

        EndWave(EventType.OnDisabled, string.Empty); // stop "disable" wave.

        if (SetupNextWave(enableWave, eType)) {
            if (listener != null) {
                listener.WaveStart(eType, enableWaveMeta.waveSpec);
            }
        }
        SpawnFromWaveMeta(enableWaveMeta, eType);

        PropagateEventToChildSpawners(eType);
    }
    #endregion

    #region OnDisable
    void OnDisable() {
        _DisableEvent(true);
    }

    private void _DisableEvent(bool calledFromSelf) {
        var eType = EventType.OnDisabled;

        UnregisterReceiver();

        if (!IsWaveValid(disableWave, eType, calledFromSelf)) {
            return;
        }

		EndWave(EventType.OnEnabled, string.Empty); // stop "enable" wave.

        if (SetupNextWave(disableWave, eType)) {
            if (listener != null) {
                listener.WaveStart(eType, disableWaveMeta.waveSpec);
            }
        }
        SpawnFromWaveMeta(disableWaveMeta, eType);

        PropagateEventToChildSpawners(eType);
    }
    #endregion

    #region Pooling events (OnSpawned, OnDespawned) - used by both Pool Boss and Pool Manager.
    void OnSpawned() {
        SpawnedOrAwake();

        _OnSpawned(true);
    }

    private void _OnSpawned(bool calledFromSelf) {
        var eType = EventType.OnSpawned;

        if (!IsWaveValid(spawnedWave, eType, calledFromSelf)) {
            return;
        }

		EndWave(EventType.Invisible, string.Empty); // stop "invisible" wave.

        if (SetupNextWave(spawnedWave, eType)) {
            if (listener != null) {
                listener.WaveStart(eType, spawnedWaveMeta.waveSpec);
            }
        }
        SpawnFromWaveMeta(spawnedWaveMeta, eType);

        PropagateEventToChildSpawners(eType);
    }

    void OnDespawned() {
        _OnDespawned(true);
    }

    private void _OnDespawned(bool calledFromSelf) {
        var eType = EventType.OnDespawned;

        if (!IsWaveValid(despawnedWave, eType, calledFromSelf)) {
            return;
        }

		EndWave(EventType.Visible, string.Empty); // stop "visible" wave.

        if (SetupNextWave(despawnedWave, eType)) {
            if (listener != null) {
                listener.WaveStart(eType, despawnedWaveMeta.waveSpec);
            }
        }
        SpawnFromWaveMeta(despawnedWaveMeta, eType);

        PropagateEventToChildSpawners(eType);
    }
    #endregion

    #region OnBecameVisible
    void OnBecameVisible() {
        if (this.isVisible) {
            return; // to fix Unity bug.
        }

        _OnBecameVisible(true);
    }

    private void _OnBecameVisible(bool calledFromSelf) {
        var eType = EventType.Visible;
        this.isVisible = true;

        if (!IsWaveValid(visibleWave, eType, calledFromSelf)) {
            return;
        }

		EndWave(EventType.Invisible, string.Empty); // stop "invisible" wave.

        if (SetupNextWave(visibleWave, eType)) {
            if (listener != null) {
                listener.WaveStart(eType, visibleWaveMeta.waveSpec);
            }
        }
        SpawnFromWaveMeta(visibleWaveMeta, eType);

        PropagateEventToChildSpawners(eType);
    }
    #endregion

    #region OnBecameInvisible
    void OnBecameInvisible() {
        _OnBecameInvisible(true);
    }

    private void _OnBecameInvisible(bool calledFromSelf) {
        StopOppositeWaveIfActive(visibleWave, EventType.Visible);

        var eType = EventType.Invisible;
        this.isVisible = false;

        if (!IsWaveValid(invisibleWave, eType, calledFromSelf)) {
            return;
        }

		EndWave(EventType.Visible, string.Empty); // stop "visible" wave.

        if (SetupNextWave(invisibleWave, eType)) {
            if (listener != null) {
                listener.WaveStart(eType, invisibleWaveMeta.waveSpec);
            }
        }
        SpawnFromWaveMeta(invisibleWaveMeta, eType);

        PropagateEventToChildSpawners(eType);
    }
    #endregion

    #region OnMouseEnter
    void OnMouseEnter() {
        _OnMouseEnter(true);
    }

    private void _OnMouseEnter(bool calledFromSelf) {
        var eType = EventType.MouseOver_Legacy;

		if (!IsSetToLegacyUI || !IsWaveValid(mouseOverWave, eType, calledFromSelf)) {
            return;
        }

        if (SetupNextWave(mouseOverWave, eType)) {
            if (listener != null) {
                listener.WaveStart(eType, mouseOverWaveMeta.waveSpec);
            }
        }
        SpawnFromWaveMeta(mouseOverWaveMeta, eType);

        PropagateEventToChildSpawners(eType);
    }
    #endregion

    #region OnMouseDown
    void OnMouseDown() {
        _OnMouseDown(true);
    }

    private void _OnMouseDown(bool calledFromSelf) {
        var eType = EventType.MouseClick_Legacy;

		if (!IsSetToLegacyUI || !IsWaveValid(mouseClickWave, eType, calledFromSelf)) {
            return;
        }

        if (SetupNextWave(mouseClickWave, eType)) {
            if (listener != null) {
                listener.WaveStart(eType, mouseClickWaveMeta.waveSpec);
            }
        }
        SpawnFromWaveMeta(mouseClickWaveMeta, eType);

        PropagateEventToChildSpawners(eType);
    }
    #endregion

    #region NGUI events (onClick)
    void OnClick() {
        _OnClick(true);
    }

    private void _OnClick(bool calledFromSelf) {
        var eType = EventType.OnClick_NGUI;

        if (!IsWaveValid(clickWave, eType, calledFromSelf)) {
            return;
        }

        if (SetupNextWave(clickWave, eType)) {
            if (listener != null) {
                listener.WaveStart(eType, clickWaveMeta.waveSpec);
            }
        }
        SpawnFromWaveMeta(clickWaveMeta, eType);

        PropagateEventToChildSpawners(eType);
    }
    #endregion

    #region OnCollisionEnter
    void OnCollisionEnter(Collision collision) {
        // check filters for matches if turned on
        if (collisionWave.useLayerFilter && !collisionWave.matchingLayers.Contains(collision.gameObject.layer)) {
            return;
        }

        if (collisionWave.useTagFilter && !collisionWave.matchingTags.Contains(collision.gameObject.tag)) {
            return;
        }

        _OnCollisionEnter(true);
    }

    private void _OnCollisionEnter(bool calledFromSelf) {
        var eType = EventType.OnCollision;

        if (!IsWaveValid(collisionWave, eType, calledFromSelf)) {
            return;
        }

        if (SetupNextWave(collisionWave, eType)) {
            if (listener != null) {
                listener.WaveStart(eType, collisionWaveMeta.waveSpec);
            }
        }
        SpawnFromWaveMeta(collisionWaveMeta, eType);

        PropagateEventToChildSpawners(eType);
    }
    #endregion

    #region OnTriggerEnter
    void OnTriggerEnter(Collider other) {
        // check filters for matches if turned on
        if (triggerEnterWave.useLayerFilter && !triggerEnterWave.matchingLayers.Contains(other.gameObject.layer)) {
            return;
        }

        if (triggerEnterWave.useTagFilter && !triggerEnterWave.matchingTags.Contains(other.gameObject.tag)) {
            return;
        }

        _OnTriggerEnter(true);
    }

    private void _OnTriggerEnter(bool calledFromSelf) {
        StopOppositeWaveIfActive(triggerExitWave, EventType.OnTriggerExit);

        var eType = EventType.OnTriggerEnter;

        if (!IsWaveValid(triggerEnterWave, eType, calledFromSelf)) {
            return;
        }

        if (SetupNextWave(triggerEnterWave, eType)) {
            if (listener != null) {
                listener.WaveStart(eType, triggerEnterWaveMeta.waveSpec);
            }
        }
        SpawnFromWaveMeta(triggerEnterWaveMeta, eType);

        PropagateEventToChildSpawners(eType);
    }
    #endregion

    #region OnTriggerExit
    void OnTriggerExit(Collider other) {
        // check filters for matches if turned on
        if (triggerExitWave.useLayerFilter && !triggerExitWave.matchingLayers.Contains(other.gameObject.layer)) {
            return;
        }

        if (triggerExitWave.useTagFilter && !triggerExitWave.matchingTags.Contains(other.gameObject.tag)) {
            return;
        }

        _OnTriggerExit(true);
    }

    private void _OnTriggerExit(bool calledFromSelf) {
        StopOppositeWaveIfActive(triggerEnterWave, EventType.OnTriggerEnter);

        var eType = EventType.OnTriggerExit;

        if (!IsWaveValid(triggerExitWave, eType, calledFromSelf)) {
            return;
        }

        if (SetupNextWave(triggerExitWave, eType)) {
            if (listener != null) {
                listener.WaveStart(eType, triggerExitWaveMeta.waveSpec);
            }
        }
        SpawnFromWaveMeta(triggerExitWaveMeta, eType);

        PropagateEventToChildSpawners(eType);
    }
    #endregion

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
    // not supported
#else
		void OnCollisionEnter2D(Collision2D collision) {
			// check filters for matches if turned on
			if (collision2dWave.useLayerFilter && !collision2dWave.matchingLayers.Contains(collision.gameObject.layer)) {
				return;
			}
			
			if (collision2dWave.useTagFilter && !collision2dWave.matchingTags.Contains(collision.gameObject.tag)) {
				return;
			}
			
			_OnCollision2dEnter(true);
		}
		
		private void _OnCollision2dEnter(bool calledFromSelf) {
			StopOppositeWaveIfActive(triggerExit2dWave, EventType.OnTriggerExit2D);
		
			var eType = EventType.OnCollision2D;
			
			if (!IsWaveValid(collision2dWave, eType, calledFromSelf)) {
				return;
			}
			
			if (SetupNextWave(collision2dWave, eType)) {
				if (listener != null) {
					listener.WaveStart(eType, collision2dWaveMeta.waveSpec);
				}
			}
			SpawnFromWaveMeta(collision2dWaveMeta, eType);
			
			PropagateEventToChildSpawners(eType);
		}


		void OnTriggerEnter2D(Collider2D other) {
			// check filters for matches if turned on
			if (triggerEnter2dWave.useLayerFilter && !triggerEnter2dWave.matchingLayers.Contains(other.gameObject.layer)) {
				return;
			}
			
			if (triggerEnter2dWave.useTagFilter && !triggerEnter2dWave.matchingTags.Contains(other.gameObject.tag)) {
				return;
			}
			
			_OnTriggerEnter2D(true);
		}
		
		private void _OnTriggerEnter2D(bool calledFromSelf) {
			var eType = EventType.OnTriggerEnter2D;
			
			if (!IsWaveValid(triggerEnter2dWave, eType, calledFromSelf)) {
				return;
			}
			
			if (SetupNextWave(triggerEnter2dWave, eType)) {
				if (listener != null) {
					listener.WaveStart(eType, triggerEnter2dWaveMeta.waveSpec);
				}
			}
			SpawnFromWaveMeta(triggerEnter2dWaveMeta, eType);
			
			PropagateEventToChildSpawners(eType);
		}


		void OnTriggerExit2D(Collider2D other) {
			// check filters for matches if turned on
			if (triggerExit2dWave.useLayerFilter && !triggerExit2dWave.matchingLayers.Contains(other.gameObject.layer)) {
				return;
			}
			
			if (triggerExit2dWave.useTagFilter && !triggerExit2dWave.matchingTags.Contains(other.gameObject.tag)) {
				return;
			}
			
			_OnTriggerExit2D(true);
		}
		
		private void _OnTriggerExit2D(bool calledFromSelf) {
			StopOppositeWaveIfActive(triggerEnter2dWave, EventType.OnTriggerEnter2D);

			var eType = EventType.OnTriggerExit2D;
			
			if (!IsWaveValid(triggerExit2dWave, eType, calledFromSelf)) {
				return;
			}
			
			if (SetupNextWave(triggerExit2dWave, eType)) {
				if (listener != null) {
					listener.WaveStart(eType, triggerExit2dWaveMeta.waveSpec);
				}
			}
			SpawnFromWaveMeta(triggerExit2dWaveMeta, eType);
			
			PropagateEventToChildSpawners(eType);
		}
#endif

#if UNITY_4_6 || UNITY_5_0
    #region UI Events
	public void OnPointerEnter (PointerEventData data) {
		_OnPointerEnter(true);
	}

	private void _OnPointerEnter(bool calledFromSelf) {
		var eType = EventType.PointerEnter_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityPointerEnterWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unityPointerEnterWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityPointerEnterWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityPointerEnterWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}


	public void OnPointerExit (PointerEventData data) {
		_OnPointerExit(true);
	}

	private void _OnPointerExit (bool calledFromSelf) {
		var eType = EventType.PointerExit_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityPointerExitWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unityPointerExitWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityPointerExitWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityPointerExitWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}


	public void OnPointerDown (PointerEventData data) {
		_OnPointerDown(true);
	}

	private void _OnPointerDown(bool calledFromSelf) {
		var eType = EventType.PointerDown_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityPointerDownWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unityPointerDownWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityPointerDownWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityPointerDownWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}


	public void OnPointerUp (PointerEventData data) {
		_OnPointerUp(true);
	}

	private void _OnPointerUp(bool calledFromSelf) {
		var eType = EventType.PointerUp_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityPointerUpWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unityPointerUpWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityPointerUpWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityPointerUpWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}


	public void OnDrag(PointerEventData data) {
		_OnDrag(true);
	}

	private void _OnDrag(bool calledFromSelf) {
		var eType = EventType.Drag_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityDragWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unityDragWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityDragWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityDragWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}


	public void OnDrop(PointerEventData data) {
		_OnDrop(true);
	}

	private void _OnDrop(bool calledFromSelf) {
		var eType = EventType.Drop_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityDropWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unityDropWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityDropWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityDropWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}


	public void OnScroll(PointerEventData data) {
		_OnScroll(true);
	}

	private void _OnScroll(bool calledFromSelf) {
		var eType = EventType.Scroll_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityScrollWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unityScrollWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityScrollWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityScrollWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}


	public void OnUpdateSelected(BaseEventData data) {
		_OnUpdateSelected(true);
	}

	private void _OnUpdateSelected(bool calledFromSelf) {
		var eType = EventType.UpdateSelected_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityUpdateSelectedWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unityUpdateSelectedWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityUpdateSelectedWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityUpdateSelectedWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}


	public void OnSelect(BaseEventData data) {
		_OnSelect(true);
	}

	private void _OnSelect(bool calledFromSelf) {
		var eType = EventType.Select_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unitySelectWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unitySelectWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unitySelectWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unitySelectWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}


	public void OnDeselect(BaseEventData data) {
		_OnDeselect(true);
	}

	private void _OnDeselect(bool calledFromSelf) {
		var eType = EventType.Deselect_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityDeselectWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unityDeselectWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityDeselectWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityDeselectWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}


	public void OnMove(AxisEventData data) {
		_OnMove(true);
	}

	private void _OnMove(bool calledFromSelf) {
		var eType = EventType.Move_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityMoveWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unityMoveWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityMoveWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityMoveWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}


	public void OnInitializePotentialDrag (PointerEventData data) {
		_OnInitializePotentialDrag(true);
	}

	private void _OnInitializePotentialDrag(bool calledFromSelf) {
		var eType = EventType.InitializePotentialDrag_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityInitializePotentialDragWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unityInitializePotentialDragWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityInitializePotentialDragWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityInitializePotentialDragWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}


	public void OnBeginDrag (PointerEventData data) {
		_OnBeginDrag(true);
	}

	private void _OnBeginDrag(bool calledFromSelf) {
		var eType = EventType.BeginDrag_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityBeginDragWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unityBeginDragWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityBeginDragWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityBeginDragWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}


	public void OnEndDrag(PointerEventData data) {
		_OnEndDrag(true);
	}

	private void _OnEndDrag(bool calledFromSelf) {
		var eType = EventType.EndDrag_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityEndDragWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unityEndDragWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityEndDragWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityEndDragWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}

	public void OnSubmit(BaseEventData data) {
		_OnSubmit(true);
	}

	private void _OnSubmit(bool calledFromSelf) {
		var eType = EventType.Submit_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unitySubmitWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unitySubmitWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unitySubmitWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unitySubmitWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}


	public void OnCancel(BaseEventData data) {
		_OnCancel(true);
	}

	private void _OnCancel(bool calledFromSelf) {
		var eType = EventType.Cancel_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityCancelWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unityCancelWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityCancelWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityCancelWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}

	#endregion

	#region Unity UI Events (4.6)

	private void SliderChanged(float newValue) {
		_SliderChanged(true);
	}

	private void _SliderChanged(bool calledFromSelf) {
		var eType = EventType.SliderChanged_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unitySliderChangedWave, eType, true)) {
			return;
		}
		
		if (SetupNextWave(unitySliderChangedWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unitySliderChangedWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unitySliderChangedWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}

	private void ButtonClicked() {
		_ButtonClicked(true);
	}

	private void _ButtonClicked(bool calledFromSelf) {
		var eType = EventType.ButtonClicked_uGUI;
		
		if (!IsSetToUGUI || !IsWaveValid(unityButtonClickedWave, eType, calledFromSelf)) {
			return;
		}
		
		if (SetupNextWave(unityButtonClickedWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, unityButtonClickedWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(unityButtonClickedWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}
	#endregion
#endif
	
	
	protected virtual void SpawnedOrAwake() {
		this.isVisible = false;

        // reset any in-progress waves that were despawned.
        enableWaveMeta = null;
        disableWaveMeta = null;
        visibleWaveMeta = null;
        invisibleWaveMeta = null;
        mouseOverWaveMeta = null;
        mouseClickWaveMeta = null;
        collisionWaveMeta = null;
        triggerEnterWaveMeta = null;
        triggerExitWaveMeta = null;
        spawnedWaveMeta = null;
        despawnedWaveMeta = null;
        codeTriggeredWave1Meta = null;
        codeTriggeredWave2Meta = null;
        clickWaveMeta = null;
        collision2dWaveMeta = null;
        triggerEnter2dWaveMeta = null;
        triggerExit2dWaveMeta = null;
		unitySliderChangedWaveMeta = null;
		unityButtonClickedWaveMeta = null;
		unityPointerDownWaveMeta = null;
		unityPointerUpWaveMeta = null;
		unityPointerEnterWaveMeta = null;
		unityPointerExitWaveMeta = null;
		unityDragWaveMeta = null;
		unityDropWaveMeta = null;
		unityScrollWaveMeta = null;
		unityUpdateSelectedWaveMeta = null;
		unitySelectWaveMeta = null;
		unityDeselectWaveMeta = null;
		unityMoveWaveMeta = null;
		unityInitializePotentialDragWaveMeta = null;
		unityBeginDragWaveMeta = null;
		unityEndDragWaveMeta = null;
		unitySubmitWaveMeta = null;
		unityCancelWaveMeta = null;

        // scan for and cache child spawners
        childSpawners = GetChildSpawners(Trans);
    }

    /// <summary>
    /// This method returns a list of the child Spawners, if any.
    /// </summary>
    /// <param name="_trans">The Transform of the Spawner to get child Spawners of.</param>
    /// <returns>A list of Triggered Spawner scripts for all child Spawners.</returns>
    public static List<TriggeredSpawner> GetChildSpawners(Transform _trans) {
        var childSpawn = new List<TriggeredSpawner>();
        if (_trans.childCount > 0) {
            for (var i = 0; i < _trans.childCount; i++) {
                var spawner = _trans.GetChild(i).GetComponent<TriggeredSpawner>();

                if (spawner == null || spawner.eventSourceType != SpawnerEventSource.ReceiveFromParent) {
                    continue;
                }

                childSpawn.Add(spawner);
            }
        }

        return childSpawn;
    }

    private bool SpawnerIsPaused {
        get {
            return LevelSettings.WavesArePaused && this.wavePauseBehavior == WavePauseBehavior.Disable;
        }
    }

    private bool GameIsOverForSpawner {
        get {
            return LevelSettings.IsGameOver && this.gameOverBehavior == GameOverBehavior.Disable;
        }
    }

    private bool IsWaveValid(TriggeredWaveSpecifics wave, EventType eType, bool calledFromSelf) {
        if (GameIsOverForSpawner || !wave.enableWave || !SpawnerIsActive) {
            return false;
        }

        switch (eventSourceType) {
            case SpawnerEventSource.Self:
                // just fine in all scenarios.
                break;
            case SpawnerEventSource.ReceiveFromParent:
                if (calledFromSelf) {
                    return false;
                }
                break;
            case SpawnerEventSource.None:
                return false; // disabled!
        }

        // check for limiting restraints
        switch (wave.retriggerLimitMode) {
            case RetriggerLimitMode.FrameBased:
                if (Time.frameCount - wave.trigLastFrame < wave.limitPerXFrm.Value) {
                    if (LevelSettings.IsLoggingOn) {
                        Debug.LogError(string.Format("{0} Wave of transform: '{1}' was limited by retrigger frame count setting.",
                            eType.ToString(),
                            Trans.name
                        ));
                    }
                    return false;
                }
                break;
            case RetriggerLimitMode.TimeBased:
                if (Time.time - wave.trigLastTime < wave.limitPerXSec.Value) {
                    if (LevelSettings.IsLoggingOn) {
                        Debug.LogError(string.Format("{0} Wave of transform: '{1}' was limited by retrigger time setting.",
                            eType.ToString(),
                            Trans.name
                        ));
                    }
                    return false;
                }
                break;
        }

        CheckForValidVariablesForWave(wave, eType);

        return true;
    }

    /// <summary>
    /// Returns whether a wave of the current event is currently active or not.
    /// </summary>
    /// <param name="eType">Event Type</param>
    /// <param name="customEventName">The name of the custom event (if you're stopping a custom wave only.</param>
    /// <returns>True or false</returns>
    public bool HasActiveWaveOfType(EventType eType, string customEventName) {
        switch (eType) {
            case EventType.OnEnabled:
                return enableWaveMeta != null;
            case EventType.OnDisabled:
                return disableWaveMeta != null;
            case EventType.Visible:
                return visibleWaveMeta != null;
            case EventType.Invisible:
                return invisibleWaveMeta != null;
			case EventType.MouseOver_Legacy:
                return mouseOverWaveMeta != null;
			case EventType.MouseClick_Legacy:
                return mouseClickWaveMeta != null;
            case EventType.OnCollision:
                return collisionWaveMeta != null;
            case EventType.OnTriggerEnter:
                return triggerEnterWaveMeta != null;
            case EventType.OnTriggerExit:
                return triggerExitWaveMeta != null;
            case EventType.OnSpawned:
                return spawnedWaveMeta != null;
            case EventType.OnDespawned:
                return despawnedWaveMeta != null;
            case EventType.CodeTriggered1:
                return codeTriggeredWave1Meta != null;
            case EventType.CodeTriggered2:
                return codeTriggeredWave2Meta != null;
			case EventType.OnClick_NGUI:
                return clickWaveMeta != null;
            case EventType.OnCollision2D:
                return collision2dWaveMeta != null;
            case EventType.OnTriggerEnter2D:
                return triggerEnter2dWaveMeta != null;
            case EventType.OnTriggerExit2D:
                return triggerExit2dWaveMeta != null;
			case EventType.SliderChanged_uGUI:
				return unitySliderChangedWaveMeta != null;
			case EventType.ButtonClicked_uGUI:
				return unityButtonClickedWaveMeta != null;
			case EventType.PointerDown_uGUI:
				return unityPointerDownWaveMeta != null;
			case EventType.PointerUp_uGUI:
				return unityPointerUpWaveMeta != null;
			case EventType.PointerEnter_uGUI:
				return unityPointerEnterWaveMeta != null;
			case EventType.PointerExit_uGUI:
				return unityPointerExitWaveMeta != null;
			case EventType.Drag_uGUI:
				return unityDragWaveMeta != null;
			case EventType.Drop_uGUI:
				return unityDropWaveMeta != null;
			case EventType.Scroll_uGUI:
				return unityScrollWaveMeta != null;
			case EventType.UpdateSelected_uGUI:
				return unityUpdateSelectedWaveMeta != null;
			case EventType.Select_uGUI:
				return unitySelectWaveMeta != null;
			case EventType.Deselect_uGUI:
				return unityDeselectWaveMeta != null;
			case EventType.Move_uGUI:
				return unityMoveWaveMeta != null;
			case EventType.InitializePotentialDrag_uGUI:
				return unityInitializePotentialDragWaveMeta != null;
			case EventType.BeginDrag_uGUI:
				return unityBeginDragWaveMeta != null;
			case EventType.EndDrag_uGUI:
				return unityEndDragWaveMeta != null;
			case EventType.Submit_uGUI:
				return unitySubmitWaveMeta != null;	
			case EventType.Cancel_uGUI:
				return unityCancelWaveMeta != null;
			case EventType.CustomEvent:	
				for (var i = 0; i < userDefinedEventWaveMeta.Count; i++) {
					var anEvent = userDefinedEventWaveMeta[i].waveSpec;
					if (anEvent.customEventActive && anEvent.customEventName == customEventName) {
						return true;
					}
				}

				return false;
            default:
                Debug.Log("Unknown event type: " + eType.ToString());
                return false;
        }
    }

    private bool HasActiveSpawningWave {
        get {
			for (var i = 0; i < userDefinedEventWaveMeta.Count; i++) {
				if (userDefinedEventWaveMeta[i].waveSpec.customEventActive) {
					return true;
				}
			}

            return enableWaveMeta != null
                || disableWaveMeta != null
                || visibleWaveMeta != null
                || invisibleWaveMeta != null
                || mouseOverWaveMeta != null
                || mouseClickWaveMeta != null
                || collisionWaveMeta != null
                || triggerEnterWaveMeta != null
                || triggerExitWaveMeta != null
                || spawnedWaveMeta != null
                || despawnedWaveMeta != null
                || codeTriggeredWave1Meta != null
                || codeTriggeredWave2Meta != null
                || clickWaveMeta != null
                || collision2dWaveMeta != null
                || triggerEnter2dWaveMeta != null
                || triggerExit2dWaveMeta != null
				|| unitySliderChangedWaveMeta != null
				|| unityButtonClickedWaveMeta != null
				|| unityPointerDownWaveMeta != null
				|| unityPointerUpWaveMeta != null
				|| unityPointerEnterWaveMeta != null
				|| unityPointerExitWaveMeta != null
				|| unityDragWaveMeta != null
				|| unityDropWaveMeta != null
				|| unityScrollWaveMeta != null
				|| unityUpdateSelectedWaveMeta != null
				|| unitySelectWaveMeta != null
				|| unityDeselectWaveMeta != null
				|| unityMoveWaveMeta != null
				|| unityInitializePotentialDragWaveMeta != null
				|| unityBeginDragWaveMeta != null
				|| unityEndDragWaveMeta != null
				|| unitySubmitWaveMeta != null
				|| unityCancelWaveMeta != null;
        }
    }

    void Update() {
        if (GameIsOverForSpawner || SpawnerIsPaused || !HasActiveSpawningWave || !SpawnerIsActive) {
            return;
        }

        SpawnFromWaveMeta(enableWaveMeta, EventType.OnEnabled);
        SpawnFromWaveMeta(disableWaveMeta, EventType.OnDisabled);
        SpawnFromWaveMeta(visibleWaveMeta, EventType.Visible);
        SpawnFromWaveMeta(invisibleWaveMeta, EventType.Invisible);
        SpawnFromWaveMeta(mouseOverWaveMeta, EventType.MouseOver_Legacy);
        SpawnFromWaveMeta(mouseClickWaveMeta, EventType.MouseClick_Legacy);
        SpawnFromWaveMeta(collisionWaveMeta, EventType.OnCollision);
        SpawnFromWaveMeta(triggerEnterWaveMeta, EventType.OnTriggerEnter);
        SpawnFromWaveMeta(triggerExitWaveMeta, EventType.OnTriggerExit);
        SpawnFromWaveMeta(spawnedWaveMeta, EventType.OnSpawned);
        SpawnFromWaveMeta(despawnedWaveMeta, EventType.OnDespawned);
        SpawnFromWaveMeta(codeTriggeredWave1Meta, EventType.CodeTriggered1);
        SpawnFromWaveMeta(codeTriggeredWave2Meta, EventType.CodeTriggered2);
        SpawnFromWaveMeta(clickWaveMeta, EventType.OnClick_NGUI);
        SpawnFromWaveMeta(collision2dWaveMeta, EventType.OnCollision2D);
        SpawnFromWaveMeta(triggerExit2dWaveMeta, EventType.OnTriggerEnter2D);
        SpawnFromWaveMeta(triggerExit2dWaveMeta, EventType.OnTriggerExit2D);
		SpawnFromWaveMeta(unitySliderChangedWaveMeta, EventType.SliderChanged_uGUI);
		SpawnFromWaveMeta(unityButtonClickedWaveMeta, EventType.ButtonClicked_uGUI);
		SpawnFromWaveMeta(unityPointerDownWaveMeta, EventType.PointerDown_uGUI);
		SpawnFromWaveMeta(unityPointerUpWaveMeta, EventType.PointerUp_uGUI);
		SpawnFromWaveMeta(unityPointerEnterWaveMeta, EventType.PointerEnter_uGUI);
		SpawnFromWaveMeta(unityPointerExitWaveMeta, EventType.PointerExit_uGUI);
		SpawnFromWaveMeta(unityDragWaveMeta, EventType.Drag_uGUI);
		SpawnFromWaveMeta(unityDropWaveMeta, EventType.Drop_uGUI);
		SpawnFromWaveMeta(unityScrollWaveMeta, EventType.Scroll_uGUI);
		SpawnFromWaveMeta(unityUpdateSelectedWaveMeta, EventType.UpdateSelected_uGUI);
		SpawnFromWaveMeta(unitySelectWaveMeta, EventType.Select_uGUI);
		SpawnFromWaveMeta(unityDeselectWaveMeta, EventType.Deselect_uGUI);
		SpawnFromWaveMeta(unityMoveWaveMeta, EventType.Move_uGUI);
		SpawnFromWaveMeta(unityInitializePotentialDragWaveMeta, EventType.InitializePotentialDrag_uGUI);
		SpawnFromWaveMeta(unityBeginDragWaveMeta, EventType.BeginDrag_uGUI);
		SpawnFromWaveMeta(unityEndDragWaveMeta, EventType.EndDrag_uGUI);
		SpawnFromWaveMeta(unitySubmitWaveMeta, EventType.Submit_uGUI);
		SpawnFromWaveMeta(unityCancelWaveMeta, EventType.Cancel_uGUI);

		for (var i = 0; i < userDefinedEventWaveMeta.Count; i++) {
			var anEvent = userDefinedEventWaveMeta[i];
			if (!anEvent.waveSpec.customEventActive) {
				continue;
			}

			SpawnFromWaveMeta(anEvent, EventType.CustomEvent);
		}
    }

    private bool CanRepeatWave(TriggeredWaveMetaData wave) {
        switch (wave.waveSpec.curWaveRepeatMode) {
            case WaveSpecifics.RepeatWaveMode.NumberOfRepetitions:
                return wave.waveRepetitionNumber < wave.waveSpec.maxRepeat.Value;
            case WaveSpecifics.RepeatWaveMode.Endless:
                return true;
            case WaveSpecifics.RepeatWaveMode.UntilWorldVariableAbove:
                for (var i = 0; i < wave.waveSpec.repeatPassCriteria.statMods.Count; i++) {
                    var stat = wave.waveSpec.repeatPassCriteria.statMods[i];

                    if (!WorldVariableTracker.VariableExistsInScene(stat._statName)) {
                        LevelSettings.LogIfNew(string.Format("Spawner '{0}' wants to repeat until World Variable '{1}' is a certain value, but that Variable is not in the Scene.",
                            Trans.name,
                            stat._statName));
                        return false;
                    }

                    var variable = WorldVariableTracker.GetWorldVariable(stat._statName);
                    if (variable == null) {
                        return false;
                    }
                    var varVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? variable.CurrentIntValue : variable.CurrentFloatValue;
                    var compareVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntAmt.Value : stat._modValueFloatAmt.Value;

                    if (varVal < compareVal) {
                        return true;
                    }
                }

                return false;
            case WaveSpecifics.RepeatWaveMode.UntilWorldVariableBelow:
                for (var i = 0; i < wave.waveSpec.repeatPassCriteria.statMods.Count; i++) {
                    var stat = wave.waveSpec.repeatPassCriteria.statMods[i];

                    if (!WorldVariableTracker.VariableExistsInScene(stat._statName)) {
                        LevelSettings.LogIfNew(string.Format("Spawner '{0}' wants to repeat until World Variable '{1}' is a certain value, but that Variable is not in the Scene.",
                            Trans.name,
                            stat._statName));
                        return false;
                    }

                    var variable = WorldVariableTracker.GetWorldVariable(stat._statName);
                    if (variable == null) {
                        return false;
                    }

                    var varVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? variable.CurrentIntValue : variable.CurrentFloatValue;
                    var compareVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntAmt.Value : stat._modValueFloatAmt.Value;

                    if (varVal > compareVal) {
                        return true;
                    }
                }

                return false;
            default:
                LevelSettings.LogIfNew("Handle new wave repetition type: " + wave.waveSpec.curWaveRepeatMode);
                return false;
        }
    }

    /// <summary>
    /// This method stops the currently spawning wave of an Event Type you pass in, if there is a match.
    /// </summary>
    /// <param name="eType">The Event Type of the wave to end.</param>
    /// <param name="customEventName">The name of the custom event (if you're stopping a custom wave only.</param>
    public void EndWave(EventType eType, string customEventName) {
        var isEarlyEnd = HasActiveWaveOfType(eType, customEventName);

        switch (eType) {
            case EventType.CodeTriggered1:
                codeTriggeredWave1Meta = null;
                break;
            case EventType.CodeTriggered2:
                codeTriggeredWave2Meta = null;
                break;
            case EventType.Invisible:
                invisibleWaveMeta = null;
                break;
			case EventType.MouseClick_Legacy:
                mouseClickWaveMeta = null;
                break;
			case EventType.MouseOver_Legacy:
                mouseOverWaveMeta = null;
                break;
			case EventType.OnClick_NGUI:
                clickWaveMeta = null;
                break;
            case EventType.OnCollision:
                collisionWaveMeta = null;
                break;
            case EventType.OnCollision2D:
                collision2dWaveMeta = null;
                break;
            case EventType.OnDespawned:
                despawnedWaveMeta = null;
                break;
            case EventType.OnDisabled:
                disableWaveMeta = null;
                break;
            case EventType.OnEnabled:
                enableWaveMeta = null;
                break;
            case EventType.OnSpawned:
                spawnedWaveMeta = null;
                break;
            case EventType.OnTriggerEnter:
                triggerEnterWaveMeta = null;
                break;
            case EventType.OnTriggerEnter2D:
                triggerEnter2dWaveMeta = null;
                break;
            case EventType.OnTriggerExit:
                triggerExitWaveMeta = null;
                break;
            case EventType.OnTriggerExit2D:
                triggerExit2dWaveMeta = null;
                break;
			case EventType.SliderChanged_uGUI:
				unitySliderChangedWaveMeta = null;
				break;
			case EventType.ButtonClicked_uGUI:
				unityButtonClickedWaveMeta = null;
				break;
			case EventType.PointerDown_uGUI:
				unityPointerDownWaveMeta = null;
				break;
			case EventType.PointerUp_uGUI:	
				unityPointerUpWaveMeta = null;
				break;	
			case EventType.PointerEnter_uGUI:
				unityPointerEnterWaveMeta = null;
				break;
			case EventType.PointerExit_uGUI:
				unityPointerExitWaveMeta = null;
				break;
			case EventType.Drag_uGUI:
				unityDragWaveMeta = null;
				break;
			case EventType.Drop_uGUI:
				unityDropWaveMeta = null;
				break;
			case EventType.Scroll_uGUI:
				unityScrollWaveMeta = null;
				break;
			case EventType.UpdateSelected_uGUI:
				unityUpdateSelectedWaveMeta = null;
				break;
			case EventType.Select_uGUI:
				unitySelectWaveMeta = null;
				break;
			case EventType.Deselect_uGUI:
				unityDeselectWaveMeta = null;
				break;
			case EventType.Move_uGUI:
				unityMoveWaveMeta = null;
				break;
			case EventType.InitializePotentialDrag_uGUI:
				unityInitializePotentialDragWaveMeta = null;
				break;
			case EventType.BeginDrag_uGUI:
				unityBeginDragWaveMeta = null;
				break;
			case EventType.EndDrag_uGUI:
				unityEndDragWaveMeta = null;
				break;
			case EventType.Submit_uGUI:
				unitySubmitWaveMeta = null;
				break;
			case EventType.Cancel_uGUI:
				unityCancelWaveMeta = null;
				break;
            case EventType.Visible:
                visibleWaveMeta = null;
                break;
			case EventType.CustomEvent:
				for (var i = 0; i < userDefinedEventWaveMeta.Count; i++) {
					var anEvent = userDefinedEventWaveMeta[i];
					if (anEvent.waveSpec.customEventActive && anEvent.waveSpec.customEventName == customEventName) {
						userDefinedEventWaveMeta.Remove(anEvent);	
						break;
					}
				}
				break;
            default:
                Debug.LogError("Illegal event: " + eType.ToString());
                return;
        }

        if (this.listener != null && isEarlyEnd) {
            this.listener.WaveEndedEarly(eType);
        }

        PropagateEndWaveToChildSpawners(eType, customEventName);
    }

    private void PropagateEndWaveToChildSpawners(EventType eType, string customEventName) {
        if (!transmitEventsToChildren) {
            return;
        }

        if (childSpawners.Count > 0) {
            if (listener != null) {
                listener.PropagatedWaveEndedEarly(eType, customEventName, Trans, childSpawners.Count);
            }

            for (var i = 0; i < childSpawners.Count; i++) {
                childSpawners[i].EndWave(eType, customEventName);
            }
        }
    }

    private void SpawnFromWaveMeta(TriggeredWaveMetaData wave, EventType eType) {
        if (wave == null || SpawnerIsPaused) {
            return;
        }

        if (wave.waveFinishedSpawning
            || (Time.time - wave.waveStartTime < wave.waveSpec.WaveDelaySec.Value)
            || (Time.time - wave.lastSpawnTime <= wave.singleSpawnTime && wave.singleSpawnTime > Time.deltaTime)) {

            if (wave.waveFinishedSpawning
                && wave.waveSpec.enableRepeatWave
                && CanRepeatWave(wave)
                && Time.time - wave.previousWaveEndTime > wave.waveSpec.repeatWavePauseSec.Value) {

                if (SetupNextWave(wave.waveSpec, eType, wave.waveRepetitionNumber)) {
                    if (listener != null) {
                        listener.WaveRepeat(eType, wave.waveSpec);
                    }
                }
            }

            return;
        }

        int numberToSpawn = 1;

        if (wave.currentWaveSize > 0) {
            if (wave.singleSpawnTime < Time.deltaTime) {
                if (wave.singleSpawnTime == 0) {
                    numberToSpawn = wave.currentWaveSize;
                } else {
                    numberToSpawn = (int)Math.Ceiling(Time.deltaTime / wave.singleSpawnTime);
                }
            }
        } else {
            numberToSpawn = 0;
        }

        for (var i = 0; i < numberToSpawn; i++) {
            if (this.CanSpawnOne()) {
                Transform prefabToSpawn = this.GetSpawnable(wave);
                if (wave.waveSpec.spawnSource == WaveSpecifics.SpawnOrigin.PrefabPool && prefabToSpawn == null) {
                    // "no item"
                    continue;
                }
                if (prefabToSpawn == null) {
                    LevelSettings.LogIfNew(string.Format("Triggered Spawner '{0}' has no prefab to spawn for event: {1}",
                        this.name,
                        eType.ToString()));

                    switch (eType) {
                        case EventType.OnEnabled:
                            EndWave(EventType.OnEnabled, wave.waveSpec.customEventName);
                            break;
                        case EventType.OnDisabled:
							EndWave(EventType.OnDisabled, wave.waveSpec.customEventName);
                            break;
                        case EventType.Visible:
							EndWave(EventType.Visible, wave.waveSpec.customEventName);
                            break;
                        case EventType.Invisible:
							EndWave(EventType.Invisible, wave.waveSpec.customEventName);
                            break;
						case EventType.MouseOver_Legacy:
							EndWave(EventType.MouseOver_Legacy, wave.waveSpec.customEventName);
                            break;
						case EventType.MouseClick_Legacy:
							EndWave(EventType.MouseClick_Legacy, wave.waveSpec.customEventName);
                            break;
                        case EventType.OnCollision:
							EndWave(EventType.OnCollision, wave.waveSpec.customEventName);
                            break;
                        case EventType.OnTriggerEnter:
							EndWave(EventType.OnTriggerEnter, wave.waveSpec.customEventName);
                            break;
                        case EventType.OnTriggerExit:
							EndWave(EventType.OnTriggerExit, wave.waveSpec.customEventName);
                            break;
                        case EventType.OnSpawned:
							EndWave(EventType.OnSpawned, wave.waveSpec.customEventName);
                            break;
                        case EventType.OnDespawned:
							EndWave(EventType.OnDespawned, wave.waveSpec.customEventName);
                            break;
                        case EventType.CodeTriggered1:
							EndWave(EventType.CodeTriggered1, wave.waveSpec.customEventName);
                            break;
                        case EventType.CodeTriggered2:
							EndWave(EventType.CodeTriggered2, wave.waveSpec.customEventName);
                            break;
						case EventType.OnClick_NGUI:
							EndWave(EventType.OnClick_NGUI, wave.waveSpec.customEventName);
                            break;
                        case EventType.OnCollision2D:
							EndWave(EventType.OnCollision2D, wave.waveSpec.customEventName);
                            break;
                        case EventType.OnTriggerEnter2D:
							EndWave(EventType.OnTriggerEnter2D, wave.waveSpec.customEventName);
                            break;
                        case EventType.OnTriggerExit2D:
							EndWave(EventType.OnTriggerExit2D, wave.waveSpec.customEventName);
                            break;
						case EventType.CustomEvent:
							EndWave(EventType.CustomEvent, wave.waveSpec.customEventName);
							break;
						case EventType.BeginDrag_uGUI:
							EndWave(EventType.BeginDrag_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.ButtonClicked_uGUI:
							EndWave(EventType.ButtonClicked_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.Cancel_uGUI:
							EndWave(EventType.Cancel_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.Deselect_uGUI:
							EndWave(EventType.Deselect_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.Drag_uGUI:
							EndWave(EventType.Drag_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.Drop_uGUI:
							EndWave(EventType.Drop_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.EndDrag_uGUI:
							EndWave(EventType.EndDrag_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.InitializePotentialDrag_uGUI:
							EndWave(EventType.InitializePotentialDrag_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.PointerDown_uGUI:
							EndWave(EventType.PointerDown_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.PointerEnter_uGUI:
							EndWave(EventType.PointerEnter_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.PointerExit_uGUI:
							EndWave(EventType.PointerExit_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.PointerUp_uGUI:
							EndWave(EventType.PointerUp_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.Scroll_uGUI:
							EndWave(EventType.Scroll_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.Select_uGUI:
							EndWave(EventType.Select_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.SliderChanged_uGUI:
							EndWave(EventType.SliderChanged_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.Submit_uGUI:
							EndWave(EventType.Submit_uGUI, wave.waveSpec.customEventName);
							break;
						case EventType.UpdateSelected_uGUI:
							EndWave(EventType.UpdateSelected_uGUI, wave.waveSpec.customEventName);
							break;
						default:
                            LevelSettings.LogIfNew("need event stop code for event: " + eType.ToString());
                            break;
                    }

                    return;
                }

                var spawnPosition = this.GetSpawnPosition(Trans.position, wave.countSpawned, wave);

                var spawnedPrefab = PoolBoss.SpawnInPool(prefabToSpawn,
                    spawnPosition, this.GetSpawnRotation(prefabToSpawn, wave.countSpawned, wave));
				
                if (!LevelSettings.AppIsShuttingDown) {
                    if (spawnedPrefab == null) {
                        if (listener != null) {
                            listener.ItemFailedToSpawn(eType, prefabToSpawn);
                        }

                        LevelSettings.LogIfNew("Could not spawn: " + prefabToSpawn);

                        return;
                    } else {
						SpawnUtility.RecordSpawnerObjectIfKillable(spawnedPrefab, this.go);
                    }
                }
				
                this.AfterSpawn(spawnedPrefab, wave, eType);
            }

            wave.countSpawned++;

            if (wave.countSpawned >= wave.currentWaveSize) {
                if (LevelSettings.IsLoggingOn) {
                    Debug.Log(string.Format("Triggered Spawner '{0}' finished spawning wave from event: {1}.",
                        this.name,
                        eType.ToString()));
                }
                wave.waveFinishedSpawning = true;
                if (wave.waveSpec.disableAfterFirstTrigger) {
                    wave.waveSpec.enableWave = false;
                }
                if (listener != null) {
                    listener.WaveFinishedSpawning(eType, wave.waveSpec);
                }

                if (wave.waveSpec.enableRepeatWave) {
                    wave.previousWaveEndTime = Time.time;
                    wave.waveRepetitionNumber++;
                }
            }

            wave.lastSpawnTime = Time.time;
        }

        AfterSpawnWave(wave);
    }

    private void AfterSpawnWave(TriggeredWaveMetaData newWave) {
        if (newWave.waveSpec.willDespawnOnEvent) {
            if (listener != null) {
                listener.SpawnerDespawning(Trans);
            }
            PoolBoss.Despawn(Trans);
        }
    }

    private bool SetupNextWave(TriggeredWaveSpecifics newWave, EventType eventType, int repetionNumber = 0) {
		if (!newWave.enableWave) { // even in repeating waves we need to check.
			return false;
        }

        if (LevelSettings.IsLoggingOn) {
            Debug.Log(string.Format("Starting wave from triggered spawner: {0}, event: {1}.",
                this.name,
                eventType.ToString()));
        }

        // award bonuses
        if (newWave.waveSpawnBonusesEnabled && (repetionNumber == 0 || newWave.useWaveSpawnBonusForRepeats)) {
            WorldVariableModifier mod = null;

            for (var i = 0; i < newWave.waveSpawnVariableModifiers.statMods.Count; i++) {
                mod = newWave.waveSpawnVariableModifiers.statMods[i];
                WorldVariableTracker.ModifyPlayerStat(mod, Trans);
            }
        }

        WavePrefabPool myWavePool = null;

        if (newWave.spawnSource == WaveSpecifics.SpawnOrigin.PrefabPool) {
            var poolTrans = LevelSettings.GetFirstMatchingPrefabPool(newWave.prefabPoolName);
            if (poolTrans == null) {
                LevelSettings.LogIfNew(string.Format("Spawner '{0}' event: {1} is trying to use a Prefab Pool that can't be found.",
                    this.name,
                    eventType.ToString()));
                return false;
            }

            myWavePool = poolTrans;
        } else {
            myWavePool = null;
        }

		var larger = Math.Max(newWave.NumberToSpwn.Value, newWave.MaxToSpawn.Value);
		var smaller = Math.Min(newWave.NumberToSpwn.Value, newWave.MaxToSpawn.Value);

		var myCurrentWaveSize = UnityEngine.Random.Range(smaller, larger + 1);
        myCurrentWaveSize += (repetionNumber * newWave.repeatItemInc.Value);
        if (newWave.enableRepeatWave) {
            myCurrentWaveSize = Math.Min(myCurrentWaveSize, newWave.repeatItemLmt.Value); // cannot exceed limits
        }
        myCurrentWaveSize = Math.Max(0, myCurrentWaveSize);

        var timeToSpawnWave = (float)newWave.TimeToSpawnEntireWave.Value;
        timeToSpawnWave += repetionNumber * newWave.repeatTimeInc.Value;
        timeToSpawnWave = Math.Min(timeToSpawnWave, newWave.repeatTimeLmt.Value); // cannot exceed limits
        timeToSpawnWave = Math.Max(0f, timeToSpawnWave);

        var mySingleSpawnTime = timeToSpawnWave / (float)myCurrentWaveSize;

        var newMetaWave = new TriggeredWaveMetaData() {
            wavePool = myWavePool,
            currentWaveSize = myCurrentWaveSize,
            waveStartTime = Time.time,
            singleSpawnTime = mySingleSpawnTime,
            waveSpec = newWave,
            waveRepetitionNumber = repetionNumber
        };

        if (newWave.enableKeepCenter) {
            var waveCalcSize = (myCurrentWaveSize - 1) * -.5f;

            newMetaWave.waveSpec.keepCenterRotation = new Vector3() {
                x = waveCalcSize * newMetaWave.waveSpec.incrementRotX.Value,
                y = waveCalcSize * newMetaWave.waveSpec.incrementRotY.Value,
                z = waveCalcSize * newMetaWave.waveSpec.incrementRotZ.Value,
            };
        } else {
            newMetaWave.waveSpec.keepCenterRotation = Vector3.zero;
        }
		
        switch (eventType) {
            case EventType.OnEnabled:
                enableWaveMeta = newMetaWave;
                break;
            case EventType.OnDisabled:
                disableWaveMeta = newMetaWave;
                break;
            case EventType.Visible:
                visibleWaveMeta = newMetaWave;
                break;
            case EventType.Invisible:
                invisibleWaveMeta = newMetaWave;
                break;
			case EventType.MouseOver_Legacy:
                mouseOverWaveMeta = newMetaWave;
                break;
			case EventType.MouseClick_Legacy:
                mouseClickWaveMeta = newMetaWave;
                break;
            case EventType.OnCollision:
                collisionWaveMeta = newMetaWave;
                break;
            case EventType.OnTriggerEnter:
                triggerEnterWaveMeta = newMetaWave;
                break;
            case EventType.OnTriggerExit:
                triggerExitWaveMeta = newMetaWave;
                break;
            case EventType.OnSpawned:
                spawnedWaveMeta = newMetaWave;
                break;
            case EventType.OnDespawned:
                despawnedWaveMeta = newMetaWave;
                break;
            case EventType.CodeTriggered1:
                codeTriggeredWave1Meta = newMetaWave;
                break;
            case EventType.CodeTriggered2:
                codeTriggeredWave2Meta = newMetaWave;
                break;
			case EventType.OnClick_NGUI:
                clickWaveMeta = newMetaWave;
                break;
            case EventType.OnCollision2D:
                collision2dWaveMeta = newMetaWave;
                break;
            case EventType.OnTriggerEnter2D:
                triggerEnter2dWaveMeta = newMetaWave;
                break;
            case EventType.OnTriggerExit2D:
                triggerExit2dWaveMeta = newMetaWave;
                break;
			case EventType.SliderChanged_uGUI:
				unitySliderChangedWaveMeta = newMetaWave;
				break;
			case EventType.ButtonClicked_uGUI:
				unityButtonClickedWaveMeta = newMetaWave;
				break;
			case EventType.PointerDown_uGUI:
				unityPointerDownWaveMeta = newMetaWave;
				break;
			case EventType.PointerUp_uGUI:
				unityPointerUpWaveMeta = newMetaWave;
				break;
			case EventType.PointerEnter_uGUI:
				unityPointerEnterWaveMeta = newMetaWave;
				break;
			case EventType.PointerExit_uGUI:
				unityPointerExitWaveMeta = newMetaWave;
				break;
			case EventType.Drag_uGUI:
				unityDragWaveMeta = newMetaWave;
				break;
			case EventType.Drop_uGUI:
				unityDropWaveMeta = newMetaWave;
				break;
			case EventType.Scroll_uGUI:
				unityScrollWaveMeta = newMetaWave;
				break;
			case EventType.UpdateSelected_uGUI:
				unityUpdateSelectedWaveMeta = newMetaWave;
				break;
			case EventType.Select_uGUI:
				unitySelectWaveMeta = newMetaWave;
				break;
			case EventType.Deselect_uGUI:
				unityDeselectWaveMeta = newMetaWave;
				break;
			case EventType.Move_uGUI:
				unityMoveWaveMeta = newMetaWave;
				break;
			case EventType.InitializePotentialDrag_uGUI:
				unityInitializePotentialDragWaveMeta = newMetaWave;
				break;
			case EventType.BeginDrag_uGUI:
				unityBeginDragWaveMeta = newMetaWave;
				break;
			case EventType.EndDrag_uGUI:
				unityEndDragWaveMeta = newMetaWave;
				break;
			case EventType.Submit_uGUI:
				unitySubmitWaveMeta = newMetaWave;
				break;
			case EventType.Cancel_uGUI:
				unityCancelWaveMeta = newMetaWave;
				break;
			case EventType.CustomEvent:
				// remove existing
				int? matchIndex = null;
				for (var i = 0; i < userDefinedEventWaveMeta.Count; i++) {
					var aWave = userDefinedEventWaveMeta[i];
					if (aWave.waveSpec.customEventName == newMetaWave.waveSpec.customEventName) {
						matchIndex = i;
						break;
					}
				}

				if (matchIndex.HasValue) {
					userDefinedEventWaveMeta.RemoveAt(matchIndex.Value);
				}

				userDefinedEventWaveMeta.Add(newMetaWave);	
			    break;
            default:
                LevelSettings.LogIfNew("No matching event type: " + eventType.ToString());
                return false;
        }

        switch (newMetaWave.waveSpec.retriggerLimitMode) {
            case RetriggerLimitMode.FrameBased:
                newMetaWave.waveSpec.trigLastFrame = Time.frameCount;
                break;
            case RetriggerLimitMode.TimeBased:
                newMetaWave.waveSpec.trigLastTime = Time.time;
                break;
        }

        return true;
    }

    protected virtual Transform GetSpawnable(TriggeredWaveMetaData wave) {
        switch (wave.waveSpec.spawnSource) {
            case WaveSpecifics.SpawnOrigin.Specific:
                return wave.waveSpec.prefabToSpawn;
            case WaveSpecifics.SpawnOrigin.PrefabPool:
                return wave.wavePool.GetRandomWeightedTransform();
        }

        return null;
    }

    protected virtual bool CanSpawnOne() {
        return true; // this is for later subclasses to override (or ones you make!)
    }

    protected virtual Vector3 GetSpawnPosition(Vector3 pos, int itemSpawnedIndex, TriggeredWaveMetaData wave) {
        if (wave.waveSpec.positionXmode == WaveSpecifics.PositionMode.CustomPosition) {
			pos.x = wave.waveSpec.customPosX.Value;
		}

		if (wave.waveSpec.positionYmode == WaveSpecifics.PositionMode.CustomPosition) {
			pos.y = wave.waveSpec.customPosY.Value;
		}
		
		if (wave.waveSpec.positionZmode == WaveSpecifics.PositionMode.CustomPosition) {
			pos.z = wave.waveSpec.customPosZ.Value;
		}
		
		var addVector = Vector3.zero;

        var currentWave = wave.waveSpec;

        addVector += new Vector3(transform.right.x * currentWave.waveOffset.x,
            transform.up.y * currentWave.waveOffset.y,
            transform.forward.z * currentWave.waveOffset.z);

        if (currentWave.enableRandomizations) {
            addVector.x = UnityEngine.Random.Range(-currentWave.randomDistX.Value, currentWave.randomDistX.Value);
            addVector.y = UnityEngine.Random.Range(-currentWave.randomDistY.Value, currentWave.randomDistY.Value);
            addVector.z = UnityEngine.Random.Range(-currentWave.randomDistZ.Value, currentWave.randomDistZ.Value);
        }

        if (currentWave.enableIncrements && itemSpawnedIndex > 0) {
            addVector.x += (currentWave.incrementPositionX.Value * itemSpawnedIndex);
            addVector.y += (currentWave.incrementPositionY.Value * itemSpawnedIndex);
            addVector.z += (currentWave.incrementPositionZ.Value * itemSpawnedIndex);
        }

        return pos + addVector;
    }

    protected virtual Quaternion GetSpawnRotation(Transform prefabToSpawn, int itemSpawnedIndex, TriggeredWaveMetaData wave) {
        var currentWave = wave.waveSpec;

        Vector3 euler = Vector3.zero;
		
        switch (currentWave.curRotationMode) {
            case WaveSpecifics.RotationMode.UsePrefabRotation:
                euler = prefabToSpawn.rotation.eulerAngles;
                break;
            case WaveSpecifics.RotationMode.UseSpawnerRotation:
                euler = Trans.rotation.eulerAngles;
                break;
            case WaveSpecifics.RotationMode.CustomRotation:
                euler = currentWave.customRotation;
                break;
			case WaveSpecifics.RotationMode.LookAtCustomEventOrigin:
				if (!currentWave.isCustomEvent) {
					Debug.LogError("Spawn Rotation Mode is set to 'Look At Custom Event Origin' but that is invalid on non-custom event. Take a look in the Inspector for '" + this.name + "'.");
					break;
				}
			
				euler = currentWave.customEventLookRotation;
				break;
        }

        if (wave.waveSpec.enableKeepCenter) {
            euler += wave.waveSpec.keepCenterRotation;
        }

        if (currentWave.enableRandomizations && currentWave.randomXRotation) {
            euler.x = UnityEngine.Random.Range(wave.waveSpec.randomXRotMin.Value, wave.waveSpec.randomXRotMax.Value);
        } else if (currentWave.enableIncrements && itemSpawnedIndex > 0) {
            euler.x += itemSpawnedIndex * currentWave.incrementRotX.Value;
        }

        if (currentWave.enableRandomizations && currentWave.randomYRotation) {
            euler.y = UnityEngine.Random.Range(wave.waveSpec.randomYRotMin.Value, wave.waveSpec.randomYRotMax.Value);
        } else if (currentWave.enableIncrements && itemSpawnedIndex > 0) {
            euler.y += itemSpawnedIndex * currentWave.incrementRotY.Value;
        }

        if (currentWave.enableRandomizations && currentWave.randomZRotation) {
            euler.z = UnityEngine.Random.Range(wave.waveSpec.randomZRotMin.Value, wave.waveSpec.randomZRotMax.Value);
        } else if (currentWave.enableIncrements && itemSpawnedIndex > 0) {
            euler.z += itemSpawnedIndex * currentWave.incrementRotZ.Value;
        }

        return Quaternion.Euler(euler);
    }

    protected virtual void AfterSpawn(Transform spawnedTrans, TriggeredWaveMetaData wave, EventType eType) {
        var currentWave = wave.waveSpec;

        if (currentWave.enablePostSpawnNudge) {
            spawnedTrans.Translate(Vector3.forward * currentWave.postSpawnNudgeFwd.Value);
            spawnedTrans.Translate(Vector3.right * currentWave.postSpawnNudgeRgt.Value);
            spawnedTrans.Translate(Vector3.down * currentWave.postSpawnNudgeDwn.Value);
        }

        switch (spawnLayerMode) {
            case WaveSyncroPrefabSpawner.SpawnLayerTagMode.UseSpawnerSettings:
                spawnedTrans.gameObject.layer = go.layer;
                break;
            case WaveSyncroPrefabSpawner.SpawnLayerTagMode.Custom:
                spawnedTrans.gameObject.layer = spawnCustomLayer;
                break;
        }

        switch (spawnTagMode) {
            case WaveSyncroPrefabSpawner.SpawnLayerTagMode.UseSpawnerSettings:
                spawnedTrans.gameObject.tag = go.tag;
                break;
            case WaveSyncroPrefabSpawner.SpawnLayerTagMode.Custom:
                spawnedTrans.gameObject.tag = spawnCustomTag;
                break;
        }

        if (listener != null) {
            listener.ItemSpawned(eType, spawnedTrans);
        }
    }

    public bool SpawnerIsActive {
        get {
            switch (activeMode) {
                case LevelSettings.ActiveItemMode.Always:
                    return true;
                case LevelSettings.ActiveItemMode.Never:
                    return false;
                case LevelSettings.ActiveItemMode.IfWorldVariableInRange:
                    if (activeItemCriteria.statMods.Count == 0) {
                        return false;
                    }

                    for (var i = 0; i < activeItemCriteria.statMods.Count; i++) {
                        var stat = activeItemCriteria.statMods[i];
                        var variable = WorldVariableTracker.GetWorldVariable(stat._statName);
                        if (variable == null) {
                            return false;
                        }
                        var varVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? variable.CurrentIntValue : variable.CurrentFloatValue;

                        var min = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntMin : stat._modValueFloatMin;
                        var max = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntMax : stat._modValueFloatMax;

                        if (min > max) {
                            LevelSettings.LogIfNew("The Min cannot be greater than the Max for Active Item Limit in Triggered Spawner '" + this.transform.name + "'.");
                            return false;
                        }

                        if (varVal < min || varVal > max) {
                            return false;
                        }
                    }

                    break;
                case LevelSettings.ActiveItemMode.IfWorldVariableOutsideRange:
                    if (activeItemCriteria.statMods.Count == 0) {
                        return false;
                    }

                    for (var i = 0; i < activeItemCriteria.statMods.Count; i++) {
                        var stat = activeItemCriteria.statMods[i];
                        var variable = WorldVariableTracker.GetWorldVariable(stat._statName);
                        if (variable == null) {
                            return false;
                        }

                        var varVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? variable.CurrentIntValue : variable.CurrentFloatValue;

                        var min = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntMin : stat._modValueFloatMin;
                        var max = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntMax : stat._modValueFloatMax;

                        if (min > max) {
                            LevelSettings.LogIfNew("The Min cannot be greater than the Max for Active Item Limit in Triggered Spawner '" + this.transform.name + "'.");
                            return false;
                        }

                        if (varVal >= min && varVal <= max) {
                            return false;
                        }
                    }

                    break;
            }

            return true;
        }
    }

    private void CheckForValidVariablesForWave(TriggeredWaveSpecifics wave, EventType eType) {
        if (!wave.enableWave) {
            return; // no need to check.
        }

        // check KillerInts for invalid types
        wave.NumberToSpwn.LogIfInvalid(Trans, "Min To Spawn", null, null, eType.ToString());
		wave.MaxToSpawn.LogIfInvalid(Trans, "Max To Spawn", null, null, eType.ToString());
		wave.maxRepeat.LogIfInvalid(Trans, "Wave Repetitions", null, null, eType.ToString());
        wave.repeatItemInc.LogIfInvalid(Trans, "Spawn Increase", null, null, eType.ToString());
        wave.repeatItemLmt.LogIfInvalid(Trans, "Spawn Limit", null, null, eType.ToString());
        wave.limitPerXFrm.LogIfInvalid(Trans, "Retrigger Min Frames Between", null, null, eType.ToString());
		
		if (wave.positionXmode == WaveSpecifics.PositionMode.CustomPosition) {
			wave.customPosX.LogIfInvalid(Trans, "Custom X Position", null, null, eType.ToString());
		}
		if (wave.positionYmode == WaveSpecifics.PositionMode.CustomPosition) {
			wave.customPosY.LogIfInvalid(Trans, "Custom Y Position", null, null, eType.ToString());
		}
		if (wave.positionZmode == WaveSpecifics.PositionMode.CustomPosition) {
			wave.customPosZ.LogIfInvalid(Trans, "Custom Z Position", null, null, eType.ToString());
		}
		
        // check KillerFloats for invalid types
        wave.WaveDelaySec.LogIfInvalid(Trans, "Delay Wave (sec)", null, null, eType.ToString());
        wave.TimeToSpawnEntireWave.LogIfInvalid(Trans, "Time To Spawn All", null, null, eType.ToString());
        wave.repeatWavePauseSec.LogIfInvalid(Trans, "Pause Before Repeat", null, null, eType.ToString());
        wave.repeatTimeInc.LogIfInvalid(Trans, "Repeat Time Increase", null, null, eType.ToString());
        wave.repeatTimeLmt.LogIfInvalid(Trans, "Repeat Time Limit", null, null, eType.ToString());
        wave.randomDistX.LogIfInvalid(Trans, "Rand. Distance X", null, null, eType.ToString());
        wave.randomDistY.LogIfInvalid(Trans, "Rand. Distance Y", null, null, eType.ToString());
        wave.randomDistZ.LogIfInvalid(Trans, "Rand. Distance Z", null, null, eType.ToString());
        wave.randomXRotMin.LogIfInvalid(Trans, "Rand. X Rot. Min", null, null, eType.ToString());
        wave.randomXRotMax.LogIfInvalid(Trans, "Rand. X Rot. Max", null, null, eType.ToString());
        wave.randomYRotMin.LogIfInvalid(Trans, "Rand. Y Rot. Min", null, null, eType.ToString());
        wave.randomYRotMax.LogIfInvalid(Trans, "Rand. Y Rot. Max", null, null, eType.ToString());
        wave.randomZRotMin.LogIfInvalid(Trans, "Rand. Z Rot. Min", null, null, eType.ToString());
        wave.randomZRotMax.LogIfInvalid(Trans, "Rand. Z Rot. Max", null, null, eType.ToString());
        wave.incrementPositionX.LogIfInvalid(Trans, "Incremental Distance X", null, null, eType.ToString());
        wave.incrementPositionY.LogIfInvalid(Trans, "Incremental Distance Y", null, null, eType.ToString());
        wave.incrementPositionZ.LogIfInvalid(Trans, "Incremental Distance Z", null, null, eType.ToString());
        wave.incrementRotX.LogIfInvalid(Trans, "Incremental Rotation X", null, null, eType.ToString());
        wave.incrementRotY.LogIfInvalid(Trans, "Incremental Rotation Y", null, null, eType.ToString());
        wave.incrementRotZ.LogIfInvalid(Trans, "Incremental Rotation Z", null, null, eType.ToString());
        wave.postSpawnNudgeFwd.LogIfInvalid(Trans, "Nudge Forward", null, null, eType.ToString());
        wave.postSpawnNudgeRgt.LogIfInvalid(Trans, "Nudge Right", null, null, eType.ToString());
        wave.postSpawnNudgeDwn.LogIfInvalid(Trans, "Nudge Down", null, null, eType.ToString());
        wave.limitPerXSec.LogIfInvalid(Trans, "Retrigger Min Seconds Between", null, null, eType.ToString());

        if (wave.curWaveRepeatMode == WaveSpecifics.RepeatWaveMode.UntilWorldVariableAbove || wave.curWaveRepeatMode == WaveSpecifics.RepeatWaveMode.UntilWorldVariableBelow) {
            for (var i = 0; i < wave.repeatPassCriteria.statMods.Count; i++) {
                var crit = wave.repeatPassCriteria.statMods[i];

                switch (crit._varTypeToUse) {
                    case WorldVariableTracker.VariableType._integer:
                        if (crit._modValueIntAmt.variableSource == LevelSettings.VariableSource.Variable) {
                            if (!WorldVariableTracker.VariableExistsInScene(crit._modValueIntAmt.worldVariableName)) {
                                if (LevelSettings.illegalVariableNames.Contains(crit._modValueIntAmt.worldVariableName)) {
                                    LevelSettings.LogIfNew(string.Format("Spawner '{0}', event '{1}' has a Repeat Item Limit criteria with no World Variable selected. Please select one.",
                                        Trans.name,
                                        eType));
                                } else {
                                    LevelSettings.LogIfNew(string.Format("Spawner '{0}', event '{1} has a Repeat Item Limit using the value of World Variable '{2}', which doesn't exist in the Scene.",
                                        Trans.name,
                                        eType,
                                        crit._modValueIntAmt.worldVariableName));
                                }
                            }
                        }

                        break;
                    case WorldVariableTracker.VariableType._float:
                        if (crit._modValueIntAmt.variableSource == LevelSettings.VariableSource.Variable) {
                            if (!WorldVariableTracker.VariableExistsInScene(crit._modValueFloatAmt.worldVariableName)) {
                                if (LevelSettings.illegalVariableNames.Contains(crit._modValueFloatAmt.worldVariableName)) {
                                    LevelSettings.LogIfNew(string.Format("Spawner '{0}', event '{1}' has a Repeat Item Limit criteria with no World Variable selected. Please select one.",
                                        Trans.name,
                                        eType));
                                } else {
                                    LevelSettings.LogIfNew(string.Format("Spawner '{0}', event '{1}' has a Repeat Item Limit using the value of World Variable '{2}', which doesn't exist in the Scene.",
                                        Trans.name,
                                        eType,
                                        crit._modValueFloatAmt.worldVariableName));
                                }
                            }
                        }

                        break;
                    default:
                        LevelSettings.LogIfNew("Add code for varType: " + crit._varTypeToUse.ToString());
                        break;
                }
            }
        }

        if (wave.waveSpawnBonusesEnabled) {
            for (var b = 0; b < wave.waveSpawnVariableModifiers.statMods.Count; b++) {
                var spawnMod = wave.waveSpawnVariableModifiers.statMods[b];

                if (WorldVariableTracker.IsBlankVariableName(spawnMod._statName)) {
                    LevelSettings.LogIfNew(string.Format("Spawner '{0}', event '{1}' specifies a Wave Spawn Bonus with no World Variable selected. Please select one.",
                        Trans.name,
                        eType));
                } else if (!WorldVariableTracker.VariableExistsInScene(spawnMod._statName)) {
                    LevelSettings.LogIfNew(string.Format("Spawner '{0}', event '{1}' specifies a Wave Spawn Bonus of World Variable '{2}', which doesn't exist in the scene.",
                        Trans.name,
                        eType,
                        spawnMod._statName));
                } else {
                    switch (spawnMod._varTypeToUse) {
                        case WorldVariableTracker.VariableType._integer:
                            if (spawnMod._modValueIntAmt.variableSource == LevelSettings.VariableSource.Variable) {
                                if (!WorldVariableTracker.VariableExistsInScene(spawnMod._modValueIntAmt.worldVariableName)) {
                                    if (LevelSettings.illegalVariableNames.Contains(spawnMod._modValueIntAmt.worldVariableName)) {
                                        LevelSettings.LogIfNew(string.Format("Spawner '{0}', event '{1}' wants to award Wave Spawn Bonus if World Variable '{2}' is above the value of an unspecified World Variable. Please select one.",
                                            Trans.name,
                                            eType,
                                            spawnMod._statName));
                                    } else {
                                        LevelSettings.LogIfNew(string.Format("Spawner '{0}', event '{1}' wants to award Wave Spawn Bonus if World Variable '{2}' is above the value of World Variable '{3}', but the latter is not in the Scene.",
                                            Trans.name,
                                            eType,
                                            spawnMod._statName,
                                            spawnMod._modValueIntAmt.worldVariableName));
                                    }
                                }
                            }

                            break;
                        case WorldVariableTracker.VariableType._float:
                            if (spawnMod._modValueFloatAmt.variableSource == LevelSettings.VariableSource.Variable) {
                                if (!WorldVariableTracker.VariableExistsInScene(spawnMod._modValueFloatAmt.worldVariableName)) {
                                    if (LevelSettings.illegalVariableNames.Contains(spawnMod._modValueFloatAmt.worldVariableName)) {
                                        LevelSettings.LogIfNew(string.Format("Spawner '{0}', event '{1}' wants to award Wave Spawn Bonus if World Variable '{2}' is above the value of an unspecified World Variable. Please select one.",
                                            Trans.name,
                                            eType,
                                            spawnMod._statName));
                                    } else {
                                        LevelSettings.LogIfNew(string.Format("Spawner '{0}', event '{1}' wants to award Wave Spawn Bonus if World Variable '{2}' is above the value of World Variable '{3}', but the latter is not in the Scene.",
                                            Trans.name,
                                            eType,
                                            spawnMod._statName,
                                            spawnMod._modValueFloatAmt.worldVariableName));
                                    }
                                }
                            }

                            break;
                        default:
                            LevelSettings.LogIfNew("Add code for varType: " + spawnMod._varTypeToUse.ToString());
                            break;
                    }
                }
            }
        }
    }

    private bool WaveIsUsingPrefabPool(TriggeredWaveSpecifics spec, string poolName) {
        if (spec.spawnSource == WaveSpecifics.SpawnOrigin.PrefabPool && spec.prefabPoolName == poolName) {
            return true;
        }

        return false;
    }

    private void StopOppositeWaveIfActive(TriggeredWaveSpecifics wave, EventType eType) {
        if (wave.enableWave && wave.stopWaveOnOppositeEvent) {
            EndWave(eType, string.Empty);
        }
    }

    public bool IsUsingPrefabPool(Transform poolTrans) {
        var poolName = poolTrans.name;
        if (WaveIsUsingPrefabPool(enableWave, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(disableWave, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(visibleWave, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(invisibleWave, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(mouseOverWave, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(mouseClickWave, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(collisionWave, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(triggerEnterWave, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(triggerExitWave, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(spawnedWave, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(despawnedWave, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(codeTriggeredWave1, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(codeTriggeredWave2, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(clickWave, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(collision2dWave, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(triggerEnter2dWave, poolName)) {
            return true;
        }
        if (WaveIsUsingPrefabPool(triggerExit2dWave, poolName)) {
            return true;
        }
		if (WaveIsUsingPrefabPool(unitySliderChangedWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityButtonClickedWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityPointerDownWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityPointerUpWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityPointerEnterWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityPointerExitWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityDragWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityDropWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityScrollWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityUpdateSelectedWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unitySelectWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityDeselectWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityMoveWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityInitializePotentialDragWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityBeginDragWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityEndDragWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unitySubmitWave, poolName)) {
			return true;
		}
		if (WaveIsUsingPrefabPool(unityCancelWave, poolName)) {
			return true;
		}

        return false;
    }

    public bool IsVisible {
        get {
            return this.isVisible;
        }
    }
	
	public Transform Trans {
		get {
			if (_trans == null) {
				_trans = this.GetComponent<Transform>();
			}
			
			return _trans;
		}
	}
	
    #region ICgkEventReceiver methods
    public virtual void CheckForIllegalCustomEvents()
    {
        for (var i = 0; i < userDefinedEventWaves.Count; i++)
        {
			var custEvent = userDefinedEventWaves[i];

            LogIfCustomEventMissing(custEvent);
        }
    }

	public virtual void ReceiveEvent(string customEventName, Vector3 eventOrigin)
    {
        for (var i = 0; i < userDefinedEventWaves.Count; i++)
        {
			var userDefWave = userDefinedEventWaves[i];
			
			if (!userDefWave.customEventActive || string.IsNullOrEmpty(userDefWave.customEventName))
            {
                continue;
            }

			if (!userDefWave.customEventName.Equals(customEventName))
            {
                continue;
            }

			if (listener != null) {	
				listener.CustomEventReceived(customEventName, eventOrigin);
			}
						
			var oldRotation = Trans.rotation;
			
			if (userDefWave.eventOriginIgnoreX) {
				eventOrigin.x = Trans.position.x;
			}
			if (userDefWave.eventOriginIgnoreY) {
				eventOrigin.x = Trans.position.y;
			}
			if (userDefWave.eventOriginIgnoreZ) {
				eventOrigin.x = Trans.position.z;
			}
			
			Trans.LookAt(eventOrigin);
			userDefWave.customEventLookRotation = Trans.rotation.eulerAngles;
			
			if (userDefWave.curSpawnerRotMode == WaveSpecifics.SpawnerRotationMode.KeepRotation) {
				Trans.rotation = oldRotation;
			}
			
			SetupNextWave(userDefWave, EventType.CustomEvent);
        }
    }

    public virtual bool SubscribesToEvent(string customEventName)
    {
        for (var i = 0; i < userDefinedEventWaves.Count; i++)
        {
			var customGrp = userDefinedEventWaves[i];
			
			if (customGrp.customEventActive && !string.IsNullOrEmpty(customGrp.customEventName) && customGrp.customEventName.Equals(customEventName))
            {
                return true;
            }
        }

        return false;
    }

    public virtual void RegisterReceiver()
    {
        if (userDefinedEventWaves.Count > 0)
        {
            LevelSettings.AddCustomEventReceiver(this, Trans);
        }
    }

    public virtual void UnregisterReceiver()
    {
        if (userDefinedEventWaves.Count > 0)
        {
            LevelSettings.RemoveCustomEventReceiver(this);
        }
    }
    #endregion

	private bool IsSetToUGUI {
		get {
			return unityUIMode != Unity_UIVersion.Legacy;
		}
	}
	
	private bool IsSetToLegacyUI {
		get {
			return unityUIMode == Unity_UIVersion.Legacy;
		}
	}

	private void LogIfCustomEventMissing(TriggeredWaveSpecifics eventGroup) {
		if (!logMissingEvents) {
			return;
		}
		
		if (!eventGroup.customEventActive || string.IsNullOrEmpty(eventGroup.customEventName)) {
			return;
		}

		string customEventName = eventGroup.customEventName;

		if (customEventName != LevelSettings.NO_EVENT_NAME && !LevelSettings.CustomEventExists(customEventName)) {
			LevelSettings.LogIfNew("Transform '" + this.name + "' is set up to receive or fire Custom Event '" + customEventName + "', which does not exist in Core GameKit.");
		}
	}
}