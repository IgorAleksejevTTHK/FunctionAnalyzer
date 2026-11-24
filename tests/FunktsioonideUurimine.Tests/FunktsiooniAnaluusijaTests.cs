using FunktsioonideUurimine.Models;
using FunktsioonideUurimine.Services;

namespace FunktsioonideUurimine.Tests;

// Testid klassile Services/FunktsiooniAnaluusija.cs.
// Oodatavad väärtused on arvutatud käsitsi (testimisaruande juhendi tabel 3),
// mitte võetud rakenduse koodist.
public class FunktsiooniAnaluusijaTests
{
    private readonly FunktsiooniAnaluusija _analuusija = new();

    // ---------- 1. Tuletise väärtus punktis ----------
    // ArvutaPunktid tagastab graafiku punktid; Dy on f'(x) väärtus selles punktis.
    [Theory]
    [InlineData("x^2 - 4*x + 3", 0, -4)]    // f'(x) = 2x - 4  ->  f'(0) = -4
    [InlineData("x^2 - 4*x + 3", 2, 0)]     // kriitiline punkt
    [InlineData("x^3 - 3*x", 2, 9)]         // f'(x) = 3x^2 - 3  ->  f'(2) = 12 - 3 = 9
    [InlineData("x^3 - 3*x", -1, 0)]        // kriitiline punkt
    [InlineData("x^3 - 12*x", 0, -12)]      // f'(x) = 3x^2 - 12  ->  f'(0) = -12
    [InlineData("x^3 - 12*x", 2, 0)]        // kriitiline punkt
    public void ArvutaPunktid_KontrollFunktsioon_TuletisVastabKasitsiArvutatule(
        string valem, double x, double oodatav)
    {
        // Act: üks punkt vahemikus [x; x + 1] sammuga 1 -> esimene punkt on x
        var punktid = _analuusija.ArvutaPunktid(valem, x, x + 1, 1);

        // Assert
        Assert.Equal(x, punktid[0].X);
        Assert.NotNull(punktid[0].Dy);
        Assert.Equal(oodatav, punktid[0].Dy!.Value, 4);
    }

    // ---------- 2. Nullkohad, kriitilised punktid ja ekstreemumid ----------
    [Theory]
    [InlineData("x^2 - 4*x + 3",
        "x1 = 1, x2 = 3",
        "x1 = 2",
        "min = -1 (x = 2)")]
    [InlineData("x^3 - 3*x",
        "x1 = -√3, x2 = 0, x3 = √3",
        "x1 = -1, x2 = 1",
        "max = 2 (x = -1), min = -2 (x = 1)")]
    [InlineData("x^3 - 12*x",
        "x1 = -2√3, x2 = 0, x3 = 2√3",
        "x1 = -2, x2 = 2",
        "max = 16 (x = -2), min = -16 (x = 2)")]
    public void Taida_KontrollFunktsioon_TulemusedVastavadTabelile3(
        string valem, string nullkohad, string kriitilised, string ekstreemumid)
    {
        var olem = new FunktsiooniUurimine { Valem = valem };

        _analuusija.Taida(olem, null);

        Assert.Equal(nullkohad, olem.Nullkohad);
        Assert.Equal(kriitilised, olem.KriitilisedPunktid);
        Assert.Equal(ekstreemumid, olem.Ekstreemumid);
    }

    // ---------- 3. Vigane valem (boonusülesanne: MathNet.Symbolics) ----------
    [Theory]
    [InlineData("x^^2")]
    [InlineData("sin(x")]
    [InlineData("3*")]
    [InlineData("y + 1")]
    public void OnKorrektne_ViganeValem_TagastabFalseJaVeateate(string valem)
    {
        bool korras = _analuusija.OnKorrektne(valem, out string viga);

        Assert.False(korras);
        Assert.False(string.IsNullOrWhiteSpace(viga));
    }

    // ---------- 4. Nulliga jagamine (boonusülesanne) ----------
    [Fact]
    public void Vaartus_UksJagatudXigaPunktisNull_EiOleMaaratud()
    {
        var tulemus = _analuusija.Vaartus("1/x", 0);

        Assert.False(tulemus.Korras);
        Assert.False(string.IsNullOrWhiteSpace(tulemus.Viga));
    }

    // ---------- 5. Erijuhud, mille käsitsi arvutatud vastus erineb rakenduse omast ----------
    // Need testid kinnitavad vearaporteid ja kukuvad testitud commit'is läbi.

    // V-02: x/x ei ole määratud kohal x = 0 (nimetaja on null), seega X = R \ {0}
    [Fact]
    public void Taida_XJagatudX_MaaramispiirkondEiSisaldaNulli()
    {
        var olem = new FunktsiooniUurimine { Valem = "x/x" };

        _analuusija.Taida(olem, null);

        Assert.Equal("X = R \\ {0}", olem.Maaramispiirkond);
    }

    // V-03: 1/(x - 100) nimetaja on null kohal x = 100, seega X = R \ {100}
    [Fact]
    public void Taida_PoolusVahemikustValjas_MaaramispiirkondEiSisalda100()
    {
        var olem = new FunktsiooniUurimine { Valem = "1/(x - 100)" };

        _analuusija.Taida(olem, null);

        Assert.Equal("X = R \\ {100}", olem.Maaramispiirkond);
    }

    // V-01: x^1000 = 0 ainult siis, kui x = 0
    [Fact]
    public void Taida_SuurAste_AinusNullkohtOnNull()
    {
        var olem = new FunktsiooniUurimine { Valem = "x^1000" };

        _analuusija.Taida(olem, null);

        Assert.Equal("x1 = 0", olem.Nullkohad);
    }
}
