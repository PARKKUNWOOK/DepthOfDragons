using UnityEngine;

public class DungeonStartManager : MonoBehaviour
{
    void Start()
    {
        string charClass = PlayerPrefs.GetString("SelectedClass", "Knight");
        int slotIndex = PlayerPrefs.GetInt("SelectedSlotIndex", -1);

        if (string.IsNullOrEmpty(charClass) || slotIndex == -1)
        {
            Debug.LogError("캐릭터 정보가 유효하지 않습니다.");
            return;
        }

        // 프리팹 로드
        GameObject prefab = Resources.Load<GameObject>($"Prefabs/Player/Character/{charClass}");
        if (prefab == null)
        {
            Debug.LogError($"프리팹 로드 실패: {charClass}");
            return;
        }

        // StartPos 찾기
        GameObject startPos = GameObject.Find("DungeonMap/StartPos");
        if (startPos == null)
        {
            Debug.LogError("DungeonMap/StartPos 오브젝트를 찾을 수 없습니다.");
            return;
        }

        // 캐릭터 스폰
        GameObject character = Instantiate(prefab);
        character.transform.position = startPos.transform.position;
        character.transform.rotation = startPos.transform.rotation;

        QuarterViewCamera cam = Camera.main.GetComponent<QuarterViewCamera>();
        if (cam != null)
        {
            cam.SetTarget(character.transform);
        }
    }
}
