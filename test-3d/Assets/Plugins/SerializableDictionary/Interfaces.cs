using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class StringIntDictionary : SerializableDictionary<string, int> {}
[Serializable] public class StringFloatDictionary : SerializableDictionary<string, float> {}
[Serializable] public class StringBoolDictionary : SerializableDictionary<string, bool> {}
[Serializable] public class StringStringDictionary : SerializableDictionary<string, string> {}
[Serializable] public class StringColorDictionary : SerializableDictionary<string, Color> {}
[Serializable] public class StringTransformDictionary : SerializableDictionary<string, Transform> {}
[Serializable] public class StringGameObjectDictionary : SerializableDictionary<string, GameObject> {}
