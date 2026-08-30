using UnityEngine;

public class BuildSocket : MonoBehaviour
{
    [SerializeField]
    private string socketId;

    [SerializeField] private BuildingType allowedType = BuildingType.GreedWall;

    public string SocketId => socketId;
    public BuildingType AllowedType => allowedType;

    public bool CanBuild(IInfectionTerritory territory, out PlacementFailure failure)
    {
        if (territory == null)
        {
            failure = PlacementFailure.OutsideInfection;
            return false;
        }
        return territory.CanPlace(allowedType, transform.position.x, out failure);
    }
}
