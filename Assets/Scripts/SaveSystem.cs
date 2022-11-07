using UnityEngine;
using System.IO;
using Assets.SimpleZip;

public class SaveSystem : MonoBehaviour
{
    [SerializeField] private string saveName = "LevelData";
    [SerializeField] private string directoryName = "Saves";

    private void OnEnable()
    {
        EventSystemNew.Subscribe(Event_Type.LOAD_LEVEL, LoadLevel);
        EventSystemNew.Subscribe(Event_Type.SAVE_LEVEL, SaveLevel);
    }

    private void OnDisable()
    {
        EventSystemNew.Unsubscribe(Event_Type.LOAD_LEVEL, LoadLevel);
        EventSystemNew.Unsubscribe(Event_Type.SAVE_LEVEL, SaveLevel);
    }

    private void SaveLevel()
    {
        AllLevelData allLevelData = new AllLevelData();

        allLevelData.tileLevelData = TileLevelManager.Instance.SaveLevel();
        allLevelData.prefabLevelData = PrefabLevelEditor.Instance.SaveLevel();

        string json = JsonUtility.ToJson(allLevelData, false);

        var compressedJson = Zip.CompressToString(json);

        File.WriteAllText(Application.dataPath + "/LevelData.txt", compressedJson);
    }

    private void LoadLevel()
    {
        string json = File.ReadAllText(Application.dataPath + "/LevelData.txt");

        var decompressedJson = Zip.Decompress(json);

        AllLevelData levelData = JsonUtility.FromJson<AllLevelData>(decompressedJson);

        TileLevelManager.Instance.LoadLevel(levelData.tileLevelData);
        PrefabLevelEditor.Instance.LoadLevel(levelData.prefabLevelData);
    }
}
