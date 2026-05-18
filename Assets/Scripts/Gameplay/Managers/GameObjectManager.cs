using UnityEngine;
using System.Collections.Generic;

public static class GameObjectManager {
    private static Dictionary<string, BigObject> bigObjects = new Dictionary<string, BigObject>();

    public static void RegisterBigObject(BigObject bigObject) {
        if (bigObject == null) {
            Debug.LogWarning("注册 BigObject 失败：对象为空");
            return;
        }

        string name = ResolveBigObjectName(bigObject);
        RegisterBigObject(name, bigObject);
    }

    public static void RegisterBigObject(string bigObjectName, BigObject bigObject) {
        if (string.IsNullOrWhiteSpace(bigObjectName)) {
            Debug.LogWarning("注册 BigObject 失败：名称为空");
            return;
        }

        if (bigObject == null) {
            Debug.LogWarning($"注册 BigObject 失败：{bigObjectName} 对象为空");
            return;
        }

        bigObjects[bigObjectName] = bigObject;
    }

    public static void UnregisterBigObject(BigObject bigObject) {
        if (bigObject == null) {
            return;
        }

        string name = ResolveBigObjectName(bigObject);
        UnregisterBigObject(name);
    }

    public static void UnregisterBigObject(string bigObjectName) {
        if (string.IsNullOrWhiteSpace(bigObjectName)) {
            return;
        }

        bigObjects.Remove(bigObjectName);
    }

    public static bool TryGetBigObject(string bigObjectName, out BigObject bigObject) {
        if (string.IsNullOrWhiteSpace(bigObjectName)) {
            bigObject = null;
            return false;
        }

        return bigObjects.TryGetValue(bigObjectName, out bigObject);
    }

    public static BigObject GetBigObject(string bigObjectName) {
        TryGetBigObject(bigObjectName, out BigObject bigObject);
        return bigObject;
    }

    public static bool TryGetSmallObject(string bigObjectName, string smallObjectName, out SmallObject smallObject) {
        smallObject = null;
        if (!TryGetBigObject(bigObjectName, out BigObject bigObject) || bigObject == null || bigObject.staticData == null) {
            return false;
        }

        if (IsSameSmallObjectName(bigObject.pastObject, smallObjectName)) {
            smallObject = bigObject.pastObject;
            return true;
        }

        if (IsSameSmallObjectName(bigObject.presentObject, smallObjectName)) {
            smallObject = bigObject.presentObject;
            return true;
        }

        return false;
    }

    public static SmallObject GetSmallObject(string bigObjectName, string smallObjectName) {
        TryGetSmallObject(bigObjectName, smallObjectName, out SmallObject smallObject);
        return smallObject;
    }

    public static bool TryGetProperty(
        string bigObjectName,
        string smallObjectName,
        string propertyName,
        out SmallObjectProperty property
    ) {
        property = null;
        if (string.IsNullOrWhiteSpace(propertyName)) {
            return false;
        }

        if (!TryGetSmallObject(bigObjectName, smallObjectName, out SmallObject smallObject) || smallObject == null) {
            return false;
        }

        if (smallObject.dynamicState == null || smallObject.dynamicState.propertyMap == null) {
            return false;
        }

        return smallObject.dynamicState.propertyMap.TryGetValue(propertyName, out property);
    }

    public static SmallObjectProperty GetProperty(string bigObjectName, string smallObjectName, string propertyName) {
        TryGetProperty(bigObjectName, smallObjectName, propertyName, out SmallObjectProperty property);
        return property;
    }

    private static string ResolveBigObjectName(BigObject bigObject) {
        return bigObject.gameObject.name;
    }

    private static bool IsSameSmallObjectName(SmallObject smallObject, string inputName) {
        if (smallObject == null || string.IsNullOrWhiteSpace(inputName)) {
            return false;
        }

        if (smallObject.staticData != null && smallObject.staticData.objectName == inputName) {
            return true;
        }

        return smallObject.gameObject.name == inputName;
    }
}