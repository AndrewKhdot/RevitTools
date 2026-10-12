# RevitTools

A set of Revit add-ins for HVAC design work, written for the tasks I kept doing by hand on real projects: numbering duct accessories, setting diffuser elevations under suspended ceilings, and checking that every air terminal has a balancing damper.

The tools are built around MagiCAD families and one office's conventions. They are not a general-purpose library; see [Limitations](#limitations).

## Commands

| Command | Ribbon button | What it does |
| --- | --- | --- |
| `FireDampersCommand` | Accessories → Przeciwpożarowe klapy | Numbers fire dampers (keeping numbers already in use) and writes type/size and the space they sit in to their parameters. |
| `SilencerAnnotationCommand` | Accessories → Tłumiki | Numbers silencers and writes size, length and pressure loss to their parameters. |
| `BalancingDampersCommand` | Accessories → Przepustnicy | Writes the space number and name to each balancing damper. |
| `CeilingDiffuserElevationCommand` | Diffusers → Wysokość nawiewników | For air terminals connected through a flex duct, finds the suspended ceiling above them in the linked architectural model and moves the terminal to that ceiling's height. Terminals with no ceiling, or more than one, are listed in the log. |
| `IsDiffuserHasARegulatorCommand` | not on the ribbon yet | Walks the duct run from each air terminal and checks for a balancing damper. Where none is found, inserts one into the first suitable round duct of that branch. Terminals it could not fix are listed in the log. **Experimental.** |
| `ChangeHeightCommand` | not on the ribbon yet | For rooms on the selected levels, finds the underside of the slab above and sets the room's Limit Offset to it. You confirm the rooms in a second dialog. |

## Requirements

- Autodesk Revit 2019 (the projects reference the 2019 API and use `DisplayUnitType`, which was removed in Revit 2022)
- .NET Framework 4.7.2
- Visual Studio 2019 or later to build
- MagiCAD families in the model — equipment is recognised by the `MC Product Code` parameter, and results are written to `MC Object Variable 1`–`4`

## Build and install

1. Open `RevitTools.sln` and build. The Revit API references point to `C:\Program Files\Autodesk\Revit 2019\`; adjust the hint paths if Revit is installed elsewhere.
2. Copy the output of the add-in project you need (its DLL, `RevitTools.Core.dll`, `RevitTools.UI.dll`, `Newtonsoft.Json.dll`) to a folder of your choice.
3. Copy `RevitTools.Core/Config/EquipmentCatalog.json` into a `Config` subfolder next to the DLLs. The commands look for it there.
4. Add a manifest to `%AppData%\Autodesk\Revit\Addins\2019\`. The repository does not include one; for the ribbon panels it looks like this:

```xml
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>RevitTools Accessories</Name>
    <Assembly>C:\path\to\RevitTools.DuctAccessoryAnnotation.dll</Assembly>
    <FullClassName>RevitTools.DuctAccessoryAnnotation.App</FullClassName>
    <AddInId>PUT-A-NEW-GUID-HERE</AddInId>
    <VendorId>RevitTools</VendorId>
  </AddIn>
</RevitAddIns>
```

`RevitTools.CeilingDiffuserElevation.App` registers the Diffusers panel the same way. The two commands without a ribbon button can be registered with `Type="Command"` or run through Add-In Manager.

## Configuration

`EquipmentCatalog.json` maps product-code prefixes to equipment kinds, separately for round and rectangular parts:

```json
"BalancingDampers": {
  "Circle": [ "PTS" ],
  "Rect":   [ "UTT/R" ],
  "Other":  []
}
```

To support another product line, add its code prefix to the right list. No rebuild is needed.

## How it is organised

| Project | Contents |
| --- | --- |
| `RevitTools.Core` | Services and models shared by all commands: collecting and filtering accessories, numbering, space lookup, duct-run traversal, ceiling intersection. |
| `RevitTools.UI` | WinForms log window and the element picker dialog. |
| `RevitTools.DuctAccessoryAnnotation` | Fire damper, silencer and balancing damper commands, plus their ribbon panel. |
| `RevitTools.CeilingDiffuserElevation` | Diffuser elevation command and the Diffusers ribbon panel. |
| `RevitTools.DiffuserDamperAssistant` | Balancing damper check and placement. |
| `RevitTools.ChangeHeight` | Room height command. |

The duct-run traversal (`MepConnectivityService.IsConnectedRecursive`) is a depth-first walk over connectors with a visited set. The caller supplies a callback that returns `Success`, `Fail` or `Continue` for each element, so the same walk serves different checks.

Logs go to the log window and to `%LocalAppData%\RevitTools\Logs\plugin.log`.

## Limitations

- **MagiCAD only.** Equipment is identified by MagiCAD parameters and product codes. Native Revit families or other manufacturers' content will not be recognised.
- **Revit 2019 only.** Later versions need the unit API calls migrated.
- **Traversal depth is 10 elements.** A damper further than that from the terminal is treated as missing.
- **Damper placement is new.** It handles round ducts in sizes 100, 125, 160, 200 and 250, places the damper at the middle of the duct, and does not set its rotation around the duct axis. Check the result before relying on it.
- **Mixed languages.** Ribbon captions and written parameter values are in Polish, log messages in English and Russian.
- **No automated tests.** Everything is checked by hand in Revit.

## Roadmap

- Level selection for the damper check
- Ribbon buttons for the remaining commands
- Support for Revit 2022+
- Longer term: trying the same checks against Autodesk Platform Services

## License

MIT — see [LICENSE.txt](LICENSE.txt).

## Author

Andrei Khadatchuk — HVAC design assistant, writing Revit automation for MEP work.
