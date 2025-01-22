## Custom Ignore Variables Pattern

when load twincat project variables, sometimes it's nice to ignore some variables, such as:

![](assets/Pasted%20image%2020250122154205.png)

- `axis.PlcToNc.xxx`

When multiple axis are used, these unnecessary variables will significantly slower the program, hence it's good to provide a filter to remove such variables. Moreover, it's better to provide a custom filter (of cource some default rules are provides)?



## Remove Multiple Variables

when a variable is passed into a function block, whether it's by `VAR_INPUT` or `VAR_IN_OUT`, the twincat ads api will return handles for the variable out and in the function block, although they are basically the identical one.

![](assets/Pasted%20image%2020250122154749.png)

To identify the duplicated variables, notice that they have same last name:
- `axis.NcToPlc.ActPos` => `NcToPlc.ActPos`
- `power_motor.Axis.NcToPlc.ActPos`=>`Axis.NcToPlc.ActPos`

the idea is to retain the variable with shortest name in the group of variables.
