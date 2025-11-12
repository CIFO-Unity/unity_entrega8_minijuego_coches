using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarsCreator : MonoBehaviour
{
    public GameObject[] vehiculos;
    public GameObject spawn1;
    private GameObject cocheClon;
    private int aleatorio;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("CrearCoche", 0.0f, 4.0f);
    }
    private void CrearCoche()
    {
        aleatorio = Random.Range(0, vehiculos.Length);
        //aleatorio = vehiculos.Length - 1;
        cocheClon = (GameObject)Instantiate(vehiculos[aleatorio], spawn1.gameObject.transform.position, Quaternion.identity);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
