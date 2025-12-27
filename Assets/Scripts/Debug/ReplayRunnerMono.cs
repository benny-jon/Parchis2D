using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReplayRunnerMono : MonoBehaviour
{
    [Header("Replay File")]
    [Tooltip("Relative path (Ex: /replays/...json)")]
    [SerializeField] public string replayFilePath;
    [SerializeField] public GameManager gameManager;

    private ReplayPlayer _player;

    private void Awake() {
        string _tag = $"{name} (id={GetInstanceID()}) scene={gameObject.scene.name}";
        Debug.Log($"[ReplayRunner] Awake {_tag} path={GetPath(transform)}");
    }

    static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void InitPlayer()
    {
        if (gameManager != null)
        {
            if (gameManager.GetStateMachine() != null)
            {
                var fullpath = Path.Combine(Application.persistentDataPath, replayFilePath);
                _player = new ReplayPlayer(fullpath, gameManager.GetStateMachine());
                Debug.Log("Replay Player Initialized. Start Now! " +  + gameObject.GetInstanceID());
            }
            else
            {
                Debug.LogError("ReplayRunner: StateMachine was NULL: " + gameObject.GetInstanceID());
            }
        }
        else
        {
            Debug.LogError("ReplayRunner: GameManager was NULL: " +  + gameObject.GetInstanceID());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isActiveAndEnabled && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (_player == null)
            {
                InitPlayer();
            }
            else
            {
                if (gameManager.CurrentGamePhase() == GamePhase.WaitingForMove || gameManager.CurrentGamePhase() == GamePhase.WaitingForRoll)
                {
                    bool executed = _player.Step();
                    if (!executed) Debug.Log("Replay Finished!: " +  + gameObject.GetInstanceID());
                }
            }
        }
    }

    private void OnDestroy() {
        _player?.Dispose();
    }
}
