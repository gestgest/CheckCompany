using UnityEngine;


//Employee 만드는 빌더 클래스
public class EmployeeBuilder
{
    public Employee BuildEmployee(EmployeeSO employeeSO, EmployeeControllerSO employeeControllerSO = null)
    {
        Employee employee;
        employee = new Employee(employeeControllerSO); 
        /*
        switch (employeeSO.GetEmployeeType())
        {
            case EmployeeType.PRODUCT_MANAGER:
                employee = new Product_Manager(); //디폴트
                break;
            case EmployeeType.DEVELOPER:
                employee = new Development();
                break;
            case EmployeeType.DESIGNER:
                employee = new Designer();
                break;
            case EmployeeType.QA:
                employee = new QA(); //디폴트
                break;
            default:
                employee = new Development(); //디폴트
                break;
        }
        */
        employee._EmployeeSO = employeeSO;
        return employee;
    }
}
