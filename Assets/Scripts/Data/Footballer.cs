using UnityEngine;

[System.Serializable]
public class Footballer
{
    public int id; // Benzersiz ID
    public string name; // Oyuncunun tam adý
    public int rating; // Oyuncu reytingi
    public Texture2D countryFlag; // Ülke bayraðý görseli
    public Texture2D teamLogo; // Takým logosu görseli
    public Texture2D footballerImage; // Oyuncu resmi
    public int price; // Oyuncu fiyatý

    // Yapýcý fonksiyon
    public Footballer(int id, string name, int rating, Texture2D countryFlag, Texture2D teamLogo, Texture2D footballerImage, int price)
    {
        this.id = id;
        this.name = name;
        this.rating = rating;
        this.countryFlag = countryFlag;
        this.teamLogo = teamLogo;
        this.footballerImage = footballerImage;
        this.price = price;
    }
}
