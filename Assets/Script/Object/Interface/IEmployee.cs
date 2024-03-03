using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IEmployee
{
    EmployeeType employeeType { get; set; }
    //이 밖에도 
    enum EmployeeType
    {
        Product_Manager,
        Developer,
        Designer,
        QA,
    }
}