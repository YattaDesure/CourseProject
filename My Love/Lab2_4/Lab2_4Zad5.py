import tkinter as tk
from tkinter import filedialog, messagebox, ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_4, вариант 8 — Задание 5")
    root.geometry("780x520")

    ttk.Label(
        root,
        text="Задание 5.\nВ файле massivsimv.txt — фамилии и инициалы студентов (по строкам).\n"
        "Для каждого посчитать однофамильцев в этой же группе.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    file_path = tk.StringVar(value="massivsimv.txt")
    row = ttk.Frame(root)
    row.pack(fill="x", padx=10, pady=6)
    ttk.Label(row, text="Файл:").pack(side="left")
    ttk.Entry(row, textvariable=file_path).pack(side="left", fill="x", expand=True, padx=8)
    ttk.Button(row, text="Выбрать...", command=lambda: pick()).pack(side="left")

    mode = tk.StringVar(value="all")  # radiobutton
    opts = ttk.LabelFrame(root, text="Кого показывать")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Всех", variable=mode, value="all").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Только у кого есть однофамильцы", variable=mode, value="dup").pack(
        side="left", padx=8, pady=6
    )

    out = tk.Text(root, height=14, wrap="word")
    out.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def pick():
        p = filedialog.askopenfilename(title="Выберите massivsimv.txt")
        if p:
            file_path.set(p)

    def run():
        try:
            with open(file_path.get().strip(), "r", encoding="utf-8") as f:
                lines = [ln.strip() for ln in f.readlines() if ln.strip()]
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))
            return

        # фамилия = первое "слово" в строке
        surnames = []
        for ln in lines:
            surn = ln.split()[0] if ln.split() else ""
            surnames.append(surn)

        out.delete("1.0", "end")
        for i, ln in enumerate(lines):
            same = 0
            for j, sn in enumerate(surnames):
                if j != i and sn == surnames[i] and surnames[i] != "":
                    same += 1
            if mode.get() == "dup" and same == 0:
                continue
            out.insert("end", f"{ln}  — однофамильцев: {same}\n")

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.mainloop()


if __name__ == "__main__":
    main()
