using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

public class DataTransformer : EditorWindow
{

    [MenuItem("Tools/ParseExcel %#K")] // Ctrl+Shift+K
    public static void ParseExcelDataToJson()
    {
        ParseExcelDataToJson<ItemDataLoader, ItemData>("Item");
        ParseExcelDataToJson<TextDataLoader, TextData>("Text");

    }

    private static void ParseExcelDataToJson<Loader, LoaderData>(string filename) where Loader : new()
    {
        // CSV 파일 경로 (Excel을 CSV로 저장한 파일)
        string csvPath = $"{Application.dataPath}/@ExcelData/{filename}Data.csv";

        if (!File.Exists(csvPath))
        {
            Debug.LogError($"CSV 파일을 찾을 수 없습니다: {csvPath}");
            Debug.Log("Excel 파일을 CSV (쉼표로 구분)로 저장한 후 다시 시도해주세요.");
            return;
        }

        try
        {
            // CSV 파일 읽기
            string[] lines = File.ReadAllLines(csvPath, Encoding.UTF8);

            if (lines.Length < 2)
            {
                Debug.LogError("CSV 파일에 데이터가 없습니다.");
                return;
            }

            // 헤더 파싱
            string[] headers = ParseCSVLine(lines[0]);

            // Loader 객체 생성
            Loader loader = new Loader();
            Type loaderType = typeof(Loader);
            Type dataType = typeof(LoaderData);


            // 필드 찾기
            FieldInfo listField = loaderType
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(field =>
                    field.FieldType.IsGenericType &&
                    field.FieldType.GetGenericTypeDefinition() == typeof(List<>) &&
                    field.FieldType.GetGenericArguments()[0] == dataType
                );

            if (listField == null)
            {
                Debug.LogError($"{loaderType.Name}에서 'items' 필드를 찾을 수 없습니다.");
                return;
            }

            // List 생성
            var dataList = listField.GetValue(loader) as System.Collections.IList;

            if (dataList == null)
            {
                Debug.LogError($"{loaderType.Name}의 '{listField.Name}' 필드가 null이거나 List 타입이 아닙니다.");
                return;
            }

            // 데이터 행 파싱 (1번 인덱스부터 시작 - 0번은 헤더)
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                string[] values = ParseCSVLine(lines[i]);

                if (values.Length != headers.Length)
                {
                    Debug.LogWarning($"라인 {i + 1}의 컬럼 수가 헤더와 일치하지 않습니다. 스킵합니다.");
                    continue;
                }

                // 데이터 객체 생성
                LoaderData data = (LoaderData)Activator.CreateInstance(dataType);

                // 필드에 값 할당
                for (int j = 0; j < headers.Length; j++)
                {
                    string headerName = headers[j].Trim();

                    FieldInfo field = dataType.GetField(
                        headerName,
                        BindingFlags.Public | BindingFlags.Instance
                    );

                    if (field == null)
                    {
                        Debug.LogWarning($"필드 '{headers[j]}'를 {dataType.Name}에서 찾을 수 없습니다.");
                        continue;
                    }

                    try
                    {
                        object convertedValue = ConvertValue(values[j], field.FieldType);
                        field.SetValue(data, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"라인 {i + 1}, 필드 '{headers[j]}' 변환 중 오류: {ex.Message}");
                    }
                }

                dataList.Add(data);
            }

            // JSON으로 변환
            string jsonData = JsonUtility.ToJson(loader, true);

            // JSON 파일 저장
            string jsonFolderPath = $"{Application.dataPath}/Resources/Data/JsonData";
            if (!Directory.Exists(jsonFolderPath))
            {
                Directory.CreateDirectory(jsonFolderPath);
            }

            string jsonPath = $"{jsonFolderPath}/{filename}Data.json";
            File.WriteAllText(jsonPath, jsonData, Encoding.UTF8);

            Debug.Log($"<color=green>JSON 파일 생성 완료: {jsonPath}</color>");
            Debug.Log($"<color=cyan>{loaderType.Name}.{listField.Name}에 총 {dataList.Count}개의 데이터가 변환되었습니다.</color>");
            // AssetDatabase 새로고침
            AssetDatabase.Refresh();
        }
        catch (Exception ex)
        {
            Debug.LogError($"파일 파싱 중 오류 발생: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        StringBuilder currentField = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // 이스케이프된 따옴표 ("")
                    currentField.Append('"');
                    i++; // 다음 따옴표 스킵
                }
                else
                {
                    // 따옴표 시작/종료
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                // 필드 구분자
                result.Add(currentField.ToString().Trim());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        // 마지막 필드 추가
        result.Add(currentField.ToString().Trim());

        return result.ToArray();
    }

    private static object ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
        {
            if (targetType == typeof(string))
                return string.Empty;
            if (targetType.IsValueType)
                return Activator.CreateInstance(targetType);
            return null;
        }

        // 기본 타입 변환
        if (targetType == typeof(int))
            return int.Parse(value);
        if (targetType == typeof(float))
            return float.Parse(value);
        if (targetType == typeof(double))
            return double.Parse(value);
        if (targetType == typeof(bool))
            return bool.Parse(value);
        if (targetType == typeof(string))
            return value;
        if (targetType == typeof(long))
            return long.Parse(value);

        // Enum 변환
        if (targetType.IsEnum)
            return Enum.Parse(targetType, value);

        return Convert.ChangeType(value, targetType);
    }

#endif
}
