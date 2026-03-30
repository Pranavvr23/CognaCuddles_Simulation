using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control : MonoBehaviour
{

    public GameObject enemy;
    public GameObject grid;


    private double currentX, currentY, prevX, prevY;
    public double velocityX, velocityY;
    // Start is called before the first frame update
    
        

    void Start()
    {
        StartCoroutine(Tick());
        currentX = enemy.transform.position.x;
        currentY = enemy.transform.position.y;
    }

    IEnumerator Tick()
    {
        while (true)
        {
            // your per-frame logic here
            UpdateGrid();

            yield return new WaitForSeconds(1f / 30f); // 30 fps
        }
    }


    // Update is called once per frame
    void UpdateGrid()
    {
        prevX = currentX;
        prevY = currentY;
        currentX = enemy.transform.position.x;
        currentY = enemy.transform.position.y;
        velocityX = (currentX - prevX) / 30f;
        velocityY = (currentY - prevY) / 30f;
    }
}
