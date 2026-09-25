using UnityEngine;

[CreateAssetMenu(
    fileName = "UFOImage",
    menuName = "Bus Game/UFO Image"
)]
public class UFOImageData : ScriptableObject
{
    public Sprite image;
    public bool isAlien;
}