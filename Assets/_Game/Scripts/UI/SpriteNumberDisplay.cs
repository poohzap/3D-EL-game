using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

// Hiển thị 1 số nguyên bằng các sprite chữ số (mỗi ký tự 1 Image)
public class SpriteNumberDisplay : MonoBehaviour
{
    [Tooltip("10 sprite chữ số, đúng thứ tự 0,1,2,...,9")]
    public Sprite[] digitSprites = new Sprite[10];
    [Tooltip("Sprite dấu phẩy (chỉ cần khi bật Use Thousands Separator)")]
    public Sprite commaSprite;

    public Vector2 digitSize = new Vector2(30, 42);
    public Vector2 commaSize = new Vector2(14, 42);
    public float spacing = 2f;

    [Tooltip("Số chữ số tối thiểu, đệm số 0 phía trước. 6 => 000042. 0 = không đệm")]
    public int minDigits = 0;
    [Tooltip("Hiện dạng 81,908. Chỉ có tác dụng khi Min Digits = 0")]
    public bool useThousandsSeparator = false;
    public TextAnchor alignment = TextAnchor.MiddleRight;

    private readonly List<Image> pool = new List<Image>();
    private int lastValue = int.MinValue;

    void Awake()
    {
        var layout = GetComponent<HorizontalLayoutGroup>();
        if (layout == null) layout = gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = alignment;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    public void SetValue(int value)
    {
        if (value == lastValue) return; // chỉ cập nhật khi số đổi
        lastValue = value;

        string s;
        if (minDigits > 0)
            s = value.ToString("D" + minDigits, CultureInfo.InvariantCulture);
        else if (useThousandsSeparator)
            s = value.ToString("N0", CultureInfo.InvariantCulture); // luôn ra dấu phẩy, không phụ thuộc ngôn ngữ máy
        else
            s = value.ToString(CultureInfo.InvariantCulture);

        while (pool.Count < s.Length) pool.Add(CreateImage());

        for (int i = 0; i < pool.Count; i++)
        {
            if (i >= s.Length) { pool[i].gameObject.SetActive(false); continue; }

            char c = s[i];
            Sprite sprite = null;
            Vector2 size = digitSize;

            if (c >= '0' && c <= '9') sprite = digitSprites[c - '0'];
            else if (c == ',') { sprite = commaSprite; size = commaSize; }

            pool[i].sprite = sprite;
            ((RectTransform)pool[i].transform).sizeDelta = size;
            pool[i].gameObject.SetActive(sprite != null);
        }
    }

    Image CreateImage()
    {
        var go = new GameObject("Char", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        var img = go.GetComponent<Image>();
        img.preserveAspect = true;
        img.raycastTarget = false;
        return img;
    }
}