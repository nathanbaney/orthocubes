using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingScript : MonoBehaviour
{
    void Start()
    {
        debugGenerateHistory();
    }

    void Update()
    {
        
    }

    void debugGenerateHistory()
    {

    }
}

public class Group //gang, corp, etc
{
    string name;
    GroupTypes groupType;
    List<Person> people;

    enum GroupTypes
    {
        Corporation,
        Gang
    }
}
public class Person
{
    string name;
    
}
