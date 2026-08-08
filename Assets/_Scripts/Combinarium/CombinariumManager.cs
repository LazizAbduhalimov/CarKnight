using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zenject;

public class CombinariumManager : MonoBehaviour
{
    [Header("Start")]
    [SerializeField] private GameObject _combinariumSlot;
    [SerializeField] private GameObject _gridPanel;
    [SerializeField] private CombinariumInfoPanel _infoPanel;

    private Dictionary<AttackComboDefinition, CombinariumSlotUI> _combiniumSlots = new Dictionary<AttackComboDefinition, CombinariumSlotUI>();
    private AttackComboTableSO _comboTableSO;
    private AttackComboService _comboService;

    private List<bool> _unlockedStates = new List<bool>();
    private string SavePath => Path.Combine(Application.persistentDataPath, "combinarium_save.json");

    [Inject]
    private void Initial(AttackComboTableSO table, AttackComboCoordinator coordinator)
    {
        _comboTableSO = table;
        LoadStates();
        FirstUpdate();
        StartCoroutine(Delay(coordinator));
    }

    IEnumerator Delay(AttackComboCoordinator coordinator)
    {
        yield return new WaitForSeconds(1);
        _comboService = coordinator._comboService;
        _comboService.ComboActivated += UpdateTask;
    }

    private void OnDisable()
    {
        _comboService.ComboActivated -= UpdateTask;
    }


    [System.Serializable]
    private class SaveData
    {
        public List<bool> unlockedStates;
    }

    private void LoadStates()
    {
        int comboCount = _comboTableSO.Combos.Count;

        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                if (data.unlockedStates != null && data.unlockedStates.Count >= comboCount)
                {
                    _unlockedStates = data.unlockedStates;
                    return;
                }
            }
            catch
            {
            }
        }

        ResetStates(comboCount);
    }

    private void ResetStates(int count)
    {
        _unlockedStates = new List<bool>();
        for (int i = 0; i < count; i++)
            _unlockedStates.Add(false);

        SaveStates();
    }

    private void SaveStates()
    {
        SaveData data = new SaveData { unlockedStates = _unlockedStates };
        File.WriteAllText(SavePath, JsonUtility.ToJson(data));
    }


    private void FirstUpdate()
    {
        _combiniumSlots?.Clear();
        for (int i = 0; i < _comboTableSO.Combos.Count; i++)
        {
            var combo = _comboTableSO.Combos[i];
            var button = Instantiate(_combinariumSlot, _gridPanel.transform)
                            .GetComponent<CombinariumSlotUI>();

            _combiniumSlots[combo] = button;
            button.DrawSlot(combo, _infoPanel, _unlockedStates[i]);
        }
    }

    public void UpdateTask(AttackComboDefinition combo)
    {
        if (!_combiniumSlots.ContainsKey(combo)) return;

        int index = -1;
        for (int i = 0; i < _comboTableSO.Combos.Count; i++)
        {
            if (_comboTableSO.Combos[i] == combo)
            {
                index = i;
                break;
            }
        }

        if (index < 0) return;

        _unlockedStates[index] = true;
        SaveStates();

        _combiniumSlots[combo].UpdateSlot(combo, true);
    }
    public static void DeleteSaveFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "combinarium_save.json");
        if (File.Exists(path))
            File.Delete(path);
    }
}