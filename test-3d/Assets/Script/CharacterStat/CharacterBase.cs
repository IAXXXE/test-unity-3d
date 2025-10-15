using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : MonoBehaviour, ICharacter
{
    public CharacterData baseData;

    public virtual void LoadCharacter(CharacterData data)
    {
        baseData = data;
    }
}
