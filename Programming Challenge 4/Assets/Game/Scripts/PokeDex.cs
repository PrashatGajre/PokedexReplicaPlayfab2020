using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;

[RequireComponent(typeof(Pokemon))]
public class PokeDex : MonoBehaviour
{
    public class Pokemon : MonoBehaviour
    {
        public string name;
        public string types;
        public Texture2D sprite;
        public string height;
        public string weight;
        public string stats;

        public Pokemon()
        { }

        public IEnumerator UpdatePokemon (JSONNode pokemonInfo)
        {
            name = pokemonInfo["name"].Value;
            types = (!string.IsNullOrWhiteSpace(pokemonInfo["types"][1]["type"]["name"].Value) ? (pokemonInfo["types"][1]["type"]["name"].Value + " , ") : "") + pokemonInfo["types"][0]["type"]["name"].Value;
            height = pokemonInfo["height"];
            weight = pokemonInfo["weight"];
            stats =
                pokemonInfo["stats"][5]["stat"]["name"].Value + " : " + pokemonInfo["stats"][5]["base_stat"] + "\n" +
                pokemonInfo["stats"][4]["stat"]["name"].Value + " : " + pokemonInfo["stats"][4]["base_stat"] + "\n" +
                pokemonInfo["stats"][3]["stat"]["name"].Value + " : " + pokemonInfo["stats"][3]["base_stat"] + "\n" +
                pokemonInfo["stats"][2]["stat"]["name"].Value + " : " + pokemonInfo["stats"][2]["base_stat"] + "\n" +
                pokemonInfo["stats"][1]["stat"]["name"].Value + " : " + pokemonInfo["stats"][1]["base_stat"] + "\n" +
                pokemonInfo["stats"][0]["stat"]["name"].Value + " : " + pokemonInfo["stats"][0]["base_stat"];
            yield return StartCoroutine(GetImageTexture(pokemonInfo["sprites"]["front_default"].Value));
        }         

        public IEnumerator GetImageTexture(string link)
        {
            UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(link);
            yield return textureRequest.SendWebRequest();

            sprite = DownloadHandlerTexture.GetContent(textureRequest);
        }


    }

    [Header("PokeDex UI")]
    [SerializeField] Text pokemonName;
    [SerializeField] Text pokemonTypes;
    [SerializeField] RawImage pokemonImage;
    [SerializeField] Text pokemonHeight;
    [SerializeField] Text pokemonWeight;
    [SerializeField] Text pokemonStats;

    //[Header("Members")]
    public Pokemon pokemon;

    private void Start()
    {
        pokemon = gameObject.AddComponent<Pokemon>();
    }

    public void FetchNewPokemon(int index)
    {
        StartCoroutine(GetPokeMonInfo(index));
    }

    IEnumerator GetPokeMonInfo(int index)
    {
        UnityWebRequest jsonRequest = UnityWebRequest.Get("https://pokeapi.co/api/v2/pokemon/" + index);
        yield return jsonRequest.SendWebRequest();

        if (jsonRequest.isNetworkError || jsonRequest.isHttpError)
        {
            Debug.Log(jsonRequest.error);
        }
        else
        {
            JSONNode pokemonInfo = JSON.Parse(jsonRequest.downloadHandler.text);
           
            yield return StartCoroutine(pokemon.UpdatePokemon(pokemonInfo));

        }
        pokemonName.text = pokemon.name.ToUpper();
        //Debug.Log("pokemonName.text = " + pokemon.name);
        pokemonTypes.text = pokemon.types.ToUpper();
        //Debug.Log("pokemonTypes.text = " + pokemon.types);
        pokemonHeight.text = "HEIGHT : " + pokemon.height;
        //Debug.Log("pokemonHeight.text = " + pokemon.height);
        pokemonWeight.text = "WEIGHT : " + pokemon.weight;
        //Debug.Log("pokemonWeight.text = " + pokemon.weight);
        pokemonStats.text = pokemon.stats.ToUpper();
        //Debug.Log("pokemonStats.text = " + pokemon.stats);
        pokemonImage.texture = pokemon.sprite;
        pokemonImage.color = Color.white;
    }

}
