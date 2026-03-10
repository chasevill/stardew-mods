using System.Reflection;
using AutomaticTodoList.Models;
using StardewModdingAPI.Events;
using StardewValley;

namespace AutomaticTodoList.Components.TodoItems;

/// <summary>A to-do entry for petting the player's house pet (cat or dog).</summary>
internal class PetTodoItem : BaseTodoItem
{
    public PetTodoItem(bool isChecked = false)
        : base(isChecked, TaskPriority.Default)
    {
    }

    public override string Text()
    {
        var pet = Game1.player.getPet();
        if (pet is null)
            return string.Empty;

        string name = GetPetName(pet);
        return I18n.Items_Animals_Single_Text(name);
    }

    public override void OnUpdateTicked(UpdateTickedEventArgs e)
    {
        if (!IsChecked)
        {
            var pet = Game1.player.getPet();
            if (pet is not null && HasBeenPetted(pet))
                this.MarkCompleted();
        }
    }

    /// <summary>Determine whether the given pet object has been petted today.</summary>
    public static bool HasBeenPetted(object pet)
    {
        // the underlying "wasPet" field is a NetBool on FarmAnimal; use reflection so we
        // don't need to worry about the concrete type (Pet/NPC) at compile time.
        var prop = pet.GetType().GetProperty("wasPet");
        if (prop is null)
            return false;

        object? netBool = prop.GetValue(pet);
        if (netBool is null)
            return false;

        var valueProp = netBool.GetType().GetProperty("Value");
        if (valueProp is null)
            return false;

        return valueProp.GetValue(netBool) is bool b && b;
    }

    /// <summary>Get a human-readable name for the pet (name assigned by the player, or
    /// fallback to the generic type).</summary>
    public override bool Equals(object? obj)
    {
        // all instances represent the same logical task (pet the house animal)
        return obj is PetTodoItem;
    }

    public override int GetHashCode()
    {
        return this.GetType().GetHashCode();
    }

    private static string GetPetName(object pet)
    {
        // try to use the displayName or name if available
        var nameProp = pet.GetType().GetProperty("displayName")
                       ?? pet.GetType().GetProperty("Name");
        if (nameProp is not null)
        {
            if (nameProp.GetValue(pet) is string s && !string.IsNullOrEmpty(s))
                return s;
        }

        // fallback to the animal type (cat/dog)
        var typeProp = pet.GetType().GetProperty("type");
        if (typeProp is not null)
        {
            object? raw = typeProp.GetValue(pet);
            if (raw is string str && !string.IsNullOrEmpty(str))
                return str;

            // sometimes this is a NetString
            var valProp = raw?.GetType().GetProperty("Value");
            if (valProp is not null && valProp.GetValue(raw) is string val && !string.IsNullOrEmpty(val))
                return val;
        }

        // worst‑case fallback
        return I18n.Items_Animals_Single_Text("pet");
    }
}
