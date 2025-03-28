using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    public GameObject generationStartPosition;
    public GameObject generationEndPosition;

    public GameObject[] objectsToInstance;

    public float boxSize = 10;

    public float generationDensity = .1f;
    public float objectSpacing = 10;

    void Start()
    {
        GenerateObjects();
    }

    // Update is called once per frame
    public void GenerateObjects()
    {
        Vector3 startPosition = generationStartPosition.transform.position;
        Vector3 endPosition = generationEndPosition.transform.position;

        float xMin = Mathf.Min(startPosition.x-boxSize, endPosition.x-boxSize);
        float xMax = Mathf.Max(startPosition.x+boxSize, endPosition.x+boxSize);
        float yMin = Mathf.Min(startPosition.y-boxSize/3, endPosition.y-boxSize/3);
        float yMax = Mathf.Max(startPosition.y+boxSize/3, endPosition.y+boxSize/3);
        float zMin = Mathf.Min(startPosition.z - boxSize, endPosition.z - boxSize);
        float zMax = Mathf.Max(startPosition.z + boxSize, endPosition.z + boxSize);

        //Estimate number of asteroids based on density to generate
        int nbObjects = Mathf.FloorToInt(generationDensity * (xMax - xMin) * (yMax - yMin) * (zMax - zMin));

        for (int i = 0; i < nbObjects; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(xMin, xMax),
                Random.Range(yMin, yMax),
                Random.Range(zMin, zMax)
            );

            Vector3 randomSize = new Vector3(Random.Range(.5f, 2.5f),Random.Range(.5f, 2.5f),Random.Range(.5f, 2.5f));

            //Pick a random object to instantiate
            GameObject objectPrefab = objectsToInstance[Random.Range(0, objectsToInstance.Length)];

            objectPrefab.transform.localScale = randomSize;

            //Check if the object do not overlap with another
            if (!Physics.CheckSphere(randomPosition, objectSpacing))
            {
                Instantiate(objectPrefab, randomPosition, Random.rotation);
            }
        }
    }
}
