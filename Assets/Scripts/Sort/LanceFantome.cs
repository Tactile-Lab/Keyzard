using UnityEngine;

public class LanceFantom : Sort
{

    public override void DestroySort(GameObject cible)
    {
        if (this.cible == cible ||cible == null)
        {
            Destroy(gameObject);
        }
        
    
    }
}
