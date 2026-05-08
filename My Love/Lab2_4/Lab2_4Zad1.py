import tkinter as tk
from tkinter import filedialog, messagebox, ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_4, вариант 8 — Задание 1")
    root.geometry("760x460")

    ttk.Label(
        root,
        text="Задание 1.\nВ файле string.txt есть строка.\nПосле каждого слова sin / cos / log поставить «(».",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    file_path = tk.StringVar(value="string.txt")

    row = ttk.Frame(root)
    row.pack(fill="x", padx=10, pady=6)
    ttk.Label(row, text="Файл:").pack(side="left")
    ttk.Entry(row, textvariable=file_path).pack(side="left", fill="x", expand=True, padx=8)
    ttk.Button(row, text="Выбрать...", command=lambda: pick()).pack(side="left")

    mode = tk.StringVar(value="all")  # radiobutton
    opts = ttk.LabelFrame(root, text="Что заменять")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="sin, cos, log", variable=mode, value="all").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="только sin/cos", variable=mode, value="trig").pack(
        side="left", padx=8, pady=6
    )

    ttk.Label(root, text="Исходный текст:").pack(anchor="w", padx=10)
    src = tk.Text(root, height=6, wrap="word")
    src.pack(fill="both", expand=True, padx=10, pady=(4, 6))

    ttk.Label(root, text="Результат:").pack(anchor="w", padx=10)
    out = tk.Text(root, height=6, wrap="word")
    out.pack(fill="both", expand=True, padx=10, pady=(4, 10))

    def pick():
        p = filedialog.askopenfilename(title="Выберите файл string.txt")
        if p:
            file_path.set(p)
            load()

    def load():
        p = file_path.get().strip()
        if not p:
            return
        try:
            with open(p, "r", encoding="utf-8") as f:
                src.delete("1.0", "end")
                src.insert("1.0", f.read())
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))

    def run():
        text = src.get("1.0", "end-1c")
        if mode.get() == "trig":
            words = ("sin", "cos")
        else:
            words = ("sin", "cos", "log")

        result = ""
        i = 0
        n = len(text)
        while i < n:
            matched = False
            for w in words:
                if text[i:i + len(w)] == w:
                    left_ok = (i == 0) or (not text[i - 1].isalpha())
                    right = i + len(w)
                    right_ch = text[right] if right < n else ""
                    right_ok = (right_ch == "") or (not right_ch.isalpha())
                    if left_ok and right_ok:
                        result += w + "("
                        i += len(w)
                        matched = True
                        break
            if not matched:
                result += text[i]
                i += 1

        out.delete("1.0", "end")
        out.insert("1.0", result)

    btns = ttk.Frame(root)
    btns.pack(fill="x", padx=10, pady=(0, 6))
    ttk.Button(btns, text="Загрузить файл", command=load).pack(side="left")
    ttk.Button(btns, text="Ок", command=run).pack(side="left", padx=8)

    root.mainloop()


if __name__ == "__main__":
    main()
