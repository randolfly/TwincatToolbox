
## TLDR 🐱‍👤

提供一个开发[Beckhoff | New Automation Technology | Beckhoff Worldwide](https://www.beckhoff.com/en-en/)的“瑞士军刀”，主要具备下面的功能：

- [ ] 连接倍福ADS Host，读写相关变量
- [ ] 按照指定频率采集变量数据，实时显示，支持数据导出
- [ ] 提供一个第三方的倍福界面HMI

## About 😸

本工具箱是在[randolfly/LabDataToolbox (github.com)](https://github.com/randolfly/LabDataToolbox)的基础上进行修改的，主要是为了解决开发倍福程序过程中的以下痛点：

1. 使用scope进行数据记录时偶尔出现==死机==的现象！
2. 想观察某个变量的值，但是不想将其显示在HMI界面上
3. 想修改某个变量的值，但是不想进入倍福的变量监控界面进行修改
4. …

如果你也遇见了上面的一系列问题，那么可以尝试一下本工具箱！

### 界面简介 🐶

![初步界面展示](./assets/Pasted%20image%2020240809223120.png)
## Todo 🐱‍🏍

- [x] 设计工具箱基础界面和代码框架

