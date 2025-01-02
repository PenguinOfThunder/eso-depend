using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EsoAdv.Metadata.Model;

public class AddonMetadata
{
    private static readonly Regex VersionSpecRe =
        new(@"^(?<name>[\-a-zA-Z0-9_.]+)(?:(?<op>>=)(?<version>\d+))?", RegexOptions.Compiled);

    private string _path;

    public AddonMetadata()
    {
    }

    public AddonMetadata(string path) : this()
    {
        Path = path;
    }

    public Dictionary<string, string> Metadata { get; } = new();

    public string Path
    {
        get => _path;
        set
        {
            _path = value;
            if (_path != null)
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(_path);
                Directory = System.IO.Path.GetDirectoryName(_path);
            }
            else
            {
                Name = null;
                Directory = null;
            }
        }
    }

    /// <summary>List of files mentioned in the manifest</summary>
    public List<string> ProvidedFiles { get; } = new();

    /// <summary>True if all mandatory fields are present</summary>
    public bool IsValid => Title != null && APIVersion != null;

    public bool IsTopLevel { get; set; }

    /// <summary>The name of the add-on, derived from the manifest filename</summary>
    public string Name { get; private set; }

    /// <summary>The directory, in which the manifest was found</summary>
    public string Directory { get; private set; }

    /**
     * <summary>
     *     ESO added the AddOnVersion: directive to support its own internal addon versioning by using the value in this
     *     directive
     *     to determine which of two identical addon, library, or dependency folders was newer; your addon folder (or a folder
     *     that is nested within your addon folder) or the folder that ESO keeps within its internal catalog.
     * </summary>
     * When this directive or its manifest file is missing, the ESO loader loads addons, libraries, and dependencies,
     * in the order listed in the dependency directives first, followed by the nested folders and/or folder directories
     * addon files in reverse alphabetic order of folder names.
     * 
     * ## AddOnVersion: 1
     * 
     * This directive is most useful for libraries which commonly get bundled with other addons but it is also necessary for any
     * stand-alone addon. Should the author accidentally copy the manifest file, this directive will help to ensure that a stand-alone
     * version of the library is preferred. Its value should be a positive integer because ESO uses the C language (
     * <c>atoi</c>
     * )
     * function to perform the numeric conversion.
     * 
     * Note: The text to numeric value conversion (C-language
     * <c>atoi</c>
     * ) actually only reads characters up to the first non digit
     * value, so 3.1 or 3bA would both be read as 3, so they would be preferred over 2. However 3.1 and 3.2 would both be resolved
     * to 3 which means the version that is touched first, would be loaded.
     */
    public int? AddOnVersion
    {
        get
        {
            if (int.TryParse(GetMetadataField("AddOnVersion"), out var addOnVersion)) return addOnVersion;
            return null;
        }
    }

    /**
     * <summary>Defines the ESO API version whose function calls were used to create this add-on.</summary>
     * This convention started at the ESO launch with API 100003. Many old Addons are still using this version.
     * The first APIVersion change to 100004 occurred when ZOS released the Craglorn/1.1 patch.
     * You can find the current APIVersion on the first line of this text file: ~\Documents\Elder Scrolls Online\live\AddOnSettings.txt
     * 
     * ## APIVersion: 100010
     * 
     * ESO changed the APIVersion directive in APIVersion 100015 to permit the metadata to contain two, six-digit, unsigned integer, values.
     * ZOS made this enhancement so developers (and users) could load and test the same add-on code changes in both game environments (Live and PTS).
     * 
     * ## APIVersion: 100015 100016
     * 
     * Currently, the ESO addon loader disables and refuses to load any add-on, library, or dependency whose an APIVersion does not match
     * the current ESO APIVersion. The most common work-around for APIVersion errors is for users (and developers) to check the
     * "Allow add-ons of other client versions" box in the Addons display window. Checking this box will let you run add-ons tested with another
     * (i.e. older) APIVersion, but checking this box will not make ZOS fix broken add-ons nor will it make the game check for broken add-ons.
     */
    public string[] APIVersion => GetMultiValueMetadataField("APIVersion");

    /// <summary>
    ///     An author or list of authors of this addon, to be displayed in the addon manager. Can contain spaces and other
    ///     special characters.
    /// </summary>
    public string Author => GetMetadataField("Author");

    public string Contributors => GetMetadataField("Contributors");
    public string Credits => GetMetadataField("Credits");

    /// <summary>A space-separated list of add-on folder names which must be loaded before this add-on is loaded.</summary>
    /// Add-on folder names are case sensitive. If a dependency name is missing from either the ESO add-ons 
    /// catalog or from the content of this add-on folder, ESO will refuse to load this add-on.
    /// After each library/AddOn name you can "optionally" add the >=
    /// <$integer>
    ///     value to do a version check against
    ///     the addon#s txt file tag ## AddOnVersion:
    ///     <$integer>
    ///         (e.g. LibAddonMenu-2.0>=28 -> Will check for LibAddonMenu-2.0txt,
    ///         tag ## AddOnVersion: 28 or higher)
    public string[] DependsOn => GetMultiValueMetadataField("DependsOn", Array.Empty<string>());

    public string[] OptionalDependsOn => GetMultiValueMetadataField("OptionalDependsOn", Array.Empty<string>());

    /// <summary>
    ///     A description of the addon that will be displayed in the tooltip for the addon in the addons window list. Can
    ///     contain spaces and other special characters.
    /// </summary>
    public string Description => GetMetadataField("Description");

    /**
     * <summary>
     *     Starting with the Murkmire Update (100025) the game periodically writes the saved variables for each loaded add-on
     *     to disk.
     *     It will attempt to write data during regular gameplay, as long as the resulting file is not bigger than 50kB
     *     and the write can be done within 4ms. Otherwise it will wait for the next loading screen.
     *     It will always wait at least two seconds between writing data to disk and 15 minutes before saving the same file
     *     again.
     *     When the DisableSavedVariablesAutoSaving is set to "1", it won't consider the saved variables of this add-on for
     *     autosaving.
     * </summary>
     * To make data save with this auto save function of ZOs you also need to use the function
     * "RequestAddOnSavedVariablesPrioritySave" in your addon so it will "register for data to save".
     */
    public string DisableSavedVariablesAutoSaving => GetMetadataField("DisableSavedVariablesAutoSaving");

    /**
     * <summary>
     *     Starting with the Elsweyr Update (100027) the game will use this directive to identify library
     *     and support add-ons and to determine which of the two sections, Add-On or Library, to list these add-ons.
     * </summary>
     * You should include this directive in the manifest files for library add-ons (e.g. those add-ons catalogued
     * and loaded by using the LibStub(r5) add-on, or intended to be a library without LibStub usage
     * (using a global variable Lib*)) and/or support add-ons (e.g. those standalone add-ons catalogued
     * by the game but loaded by using their own loading methodology).
     */
    public bool IsLibrary => string.Compare(GetMetadataField("IsLibrary"), "true", true) == 0;

    /**
     * <summary>
     *     Space-separated list of names for data objects that will be persisted to disk in between UI loads, allowing you to
     *     save data that
     *     isn't reset when the user logs out or reloads the UI. Usually accessed via ZO_SavedVars, but in essence it's just a
     *     series of global tables.
     *     However, because they are global tables, their data object names are not specific to, nor are they associated with,
     *     any one particular add-on.
     * </summary>
     * The first requirement is that you not duplicate the Saved Variables name of any other add-on. If you do something silly, as I did,
     * and use the same Saved Variables name in the manifests for two different add-ons, you will quickly find confusion in that particular data
     * object because the game will help both add-ons update the contents of that same table at random intervals with different information.
     * May I suggest that you include the name of your add-on in every Saved Variables data object name that your add-on uses to prevent
     * unforeseen and unplanned events from happening when you least expect them to occur.
     * 
     * The other requirement is that Saved Variable data objects must be created (if they don't already exist) in the EVENT_ADD_ON_LOADED
     * event handler for your add-on; otherwise, they may not be saved properly. And as I understand it, you should not try to access the
     * data objects any earlier either, as they might not be loaded yet.
     */
    public string SavedVariables => GetMetadataField("SavedVariables");


    /// <summary>A descriptive title for the addon, displayed in the addon manager.</summary>
    /// It can contain spaces and other special characters. Its length is limited to 64 characters.
    public string Title => GetMetadataField("Title");

    /**
     * <summary>
     *     Documents this addon's version identification. The content can be numbers, characters, spaces and other
     *     special characters.
     * </summary>
     * This is not an official directive but it should be present for the ESOUI addon manager, Minion https://www.minion.gg/
     */
    public string Version => GetMetadataField("Version");

    /// <summary>
    ///     Expand the filename to replace placeholders.
    ///     Supported placeholders: $(APIVersion), $(language), $(languageDirectory)
    /// </summary>
    /// $(languageDirectory) is expanded to "$(language)/"
    /// <param name="filename">the filename to expand</param>
    /// <param name="apiVersion">string value to replace APIVersion with</param>
    /// <param name="language">string value to replace language with</param>
    /// <returns>The filename with placeholders expanded</returns>
    public static string ExpandFileName(string filename, string language, int? apiVersion)
    {
        filename = filename?.Replace("$(APIVersion)", apiVersion?.ToString());
        filename = filename?.Replace("$(language)", language);
        filename = filename?.Replace("$(languageDirectory)", language + "/");
        return filename;
    }

    ///<summary>Get the value of a metadata field</summary>
    private string GetMetadataField(string name, string defaultValue = null)
    {
        if (Metadata.TryGetValue(name, out var value)) return value;
        return defaultValue;
    }


    /// <summary>Get the value of a metadata field and break it down by space-separated values into an array.</summary>
    private string[] GetMultiValueMetadataField(string name, string[] defaultValue = null)
    {
        if (Metadata.TryGetValue(name, out var value)) return value?.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        return defaultValue;
    }

    /**
     * <summary>Check if this addon satisfies a version spec, e.g.: "MyFancyAddon>=123" or just "MyFancyAddon"</summary>
     * - Names must match
     * - If version is specified, this AddOnVersion satisfy the version number according to the operator (GTE above)
     */
    public bool SatisfiesVersion(string versionSpec)
    {
        var m = VersionSpecRe.Match(versionSpec);
        if (!m.Success) throw new ArgumentException("Invalid version spec: " + versionSpec, nameof(versionSpec));

        var wantedName = m.Groups["name"]?.Value?.Trim();
        int? wantedVersionNo = null;
        if (int.TryParse(m.Groups["version"]?.Value?.Trim(), out var _version)) wantedVersionNo = _version;
        // This should be an integer             
        return wantedName == Name && (wantedVersionNo == null || AddOnVersion >= wantedVersionNo);
    }
}