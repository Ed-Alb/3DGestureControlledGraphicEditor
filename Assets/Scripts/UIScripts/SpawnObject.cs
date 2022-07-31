using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SpawnObject : MonoBehaviour
{
    public GameObject spawnPoint;
    private Camera cam;

    public Button _spawnCubeButton;
    public Button _spawnSphereButton;
    public Button _spawnCapsuleButton;
    public Button _spawnCylinderButton;

    int[] name_suffixes = { 0, 0, 0, 0 };

    private string _spawnCubeName = "InteractiveCube";
    private string _spawnSphereName = "InteractiveSphere";
    private string _spawnCapsuleName = "InteractiveCapsule";
    private string _spawnCylinderName = "InteractiveCylinder";

    [SerializeField]
    private GameObject _buttonTemplate;

    private bool _hideObjName = false;
    private List<GameObject> _spawnedObjects;

    private void Start()
    {
        _spawnedObjects = new List<GameObject>();

        cam = Camera.main;

        _spawnCubeButton.onClick.AddListener(() => Spawn(0));
        _spawnSphereButton.onClick.AddListener(() => Spawn(1));
        _spawnCylinderButton.onClick.AddListener(() => Spawn(2));
        _spawnCapsuleButton.onClick.AddListener(() => Spawn(3));
    }

    /*
     * Go through all spawned items and delete
     * their names if visible
     */
    public void HideObjectsName()
    {
        if (_spawnedObjects.Count > 0)
        {
            if (_spawnedObjects[0].GetComponent<ObjectInteraction>().getHideText())
            {
                foreach (GameObject obj in _spawnedObjects)
                {
                    //Debug.Log("Here");
                    ObjectInteraction objint = obj.GetComponent<ObjectInteraction>();
                    if (objint.getHideText())
                    {
                        DestroyObjText(objint);
                    }
                }
            }
        }

        // Just for testing
        HideDemoObjectsName();
    }

    private void HideDemoObjectsName()
    {
        ObjectInteraction demoShpere = GameObject.Find("Sphere 00").GetComponent<ObjectInteraction>();
        ObjectInteraction demoCube = GameObject.Find("Cube 00").GetComponent<ObjectInteraction>();
        ObjectInteraction demoCylinder = GameObject.Find("Cylinder 00").GetComponent<ObjectInteraction>();
        ObjectInteraction demoCapsule = GameObject.Find("Capsule 00").GetComponent<ObjectInteraction>();

        if (demoShpere.getHideText())
        {
            DestroyObjText(demoShpere);
            DestroyObjText(demoCube);
            DestroyObjText(demoCylinder);
            DestroyObjText(demoCapsule);
        }
    }

    private void DestroyObjText(ObjectInteraction obj)
    {
        Destroy(obj.getFloatingText());
        obj.setHideText(false);
    }

    // 0 - Cube, 1 - Sphere, 2 - Cylinder, 3 - Capsule
    public void Spawn(int objType)
    {
        GameObject go = null;
        switch (objType)
        {
            case 0:
                go = InstantiateObject(_spawnCubeName, objType);
                break;
            case 1:
                go = InstantiateObject(_spawnSphereName, objType);
                break;
            case 2:
                go = InstantiateObject(_spawnCylinderName, objType);
                break;
            case 3:
                go = InstantiateObject(_spawnCapsuleName, objType);
                break;
        }

        if (go)
        {
            _spawnedObjects.Add(go);
            HideObjectsName();
        }
    }

    public GameObject InstantiateObject(string objName, int arrIdx)
    {
        GameObject t = (GameObject)Resources.Load("Prefabs/" + objName, typeof(GameObject));

        t.name = objName + " " + name_suffixes[arrIdx];
        InstantiateButton(t.name);
        name_suffixes[arrIdx]++;

        GameObject obj = Instantiate(t, spawnPoint.transform);
        return obj;
    }

    public void InstantiateButton(string _buttonName)
    {
        GameObject _button = Instantiate(_buttonTemplate) as GameObject;

        string _newButtonName = _buttonName.Remove(0, "Interactive".Length);
        _button.GetComponentInChildren<TextMeshProUGUI>().text = _newButtonName;
        _button.name = _newButtonName + " Btn";

        _button.SetActive(true);
        _button.transform.SetParent(_buttonTemplate.transform.parent, false);
        _button.GetComponent<Button>().onClick.AddListener(() => FocusOnObject(_buttonName));
    }

    private void FocusOnObject(string _objectName)
    {
        GameObject selectedObj = GameObject.Find(_objectName + "(Clone)");
        Camera.main.GetComponent<FreeCam>().TriggerCameraMovement(selectedObj.transform, true, true, 4f);
    }
}
