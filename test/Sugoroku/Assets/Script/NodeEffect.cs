using UnityEngine;

public class NodeEffect : MonoBehaviour
{
    [TextArea(3, 5)] // Inspectorで複数行入力できるようにする
    public string effectDescription = "nothing"; // このマスに止まった時の効果説明

    // もし効果の種類を数値や列挙型で管理したい場合（例：1=進む、2=戻る）
    public EffectType effectType; 
    public int effectValue; // 進む・戻るマスの場合の数値

    public enum EffectType
    {
        None,       // 何もなし
        GoForward,  // 進む
        GoBackward, // 戻る
        GetItem,    // アイテムゲット
        Event       // イベントマス
    }
}