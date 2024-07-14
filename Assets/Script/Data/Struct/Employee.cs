using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Employee가 아니라 개발자, 디자이너 이런식으로 해야할 거 같은데
public class Employee : MonoBehaviour, IEmployee
{
    int id; //식별 번호

    public EmployeeType _EmployeeType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public string Name { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public int Age { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public int Career { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public int Cost { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
}
