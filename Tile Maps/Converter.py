import os
import sys
import csv

def csv_to_csharp_array(csv_path):
    base_name = os.path.splitext(csv_path)[0]
    output_path = base_name + ".txt"

    try:
        with open(csv_path, newline='') as csvfile:
            reader = csv.reader(csvfile)
            rows = [list(map(int, row)) for row in reader]

        # Format as C# int[,] array
        output_lines = ["new int[,]"]
        output_lines.append("{")
        for row in rows:
            line = "    {" + ",".join(f"{val:02d}" for val in row) + "},"
            output_lines.append(line)
        output_lines.append("}")

        # Write to output file
        with open(output_path, 'w') as outfile:
            outfile.write("\n".join(output_lines))

        print(f"Formatted C# array saved to: {output_path}")

    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python convert_csv_to_csharp.py path_to_csv_file")
    else:
        csv_to_csharp_array(sys.argv[1])
