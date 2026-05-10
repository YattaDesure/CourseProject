import tkinter as tk
from tkinter import ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_2, вариант 8 — Задание 6")
    root.geometry("760x460")

    ttk.Label(
        root,
        text="Задание 6.\n"
        "Строка из слов (русские), между словами пробелы (1 или много).\n"
        "В каждом слове удаляем все буквы, которые равны последней букве слова (кроме последней).",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    ttk.Label(root, text="Ввод:").pack(anchor="w", padx=10)
    inp = tk.Text(root, height=6, wrap="word")
    inp.pack(fill="x", padx=10, pady=(4, 8))
    inp.focus_set()

    mode = tk.StringVar(value="do")  # radiobutton
    opts = ttk.LabelFrame(root, text="Режим")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Преобразовать", variable=mode, value="do").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Ничего не делать", variable=mode, value="no").pack(
        side="left", padx=8, pady=6
    )

    ttk.Label(root, text="Вывод:").pack(anchor="w", padx=10, pady=(8, 0))
    out = tk.Text(root, height=8, wrap="word")
    out.pack(fill="both", expand=True, padx=10, pady=(4, 10))

    def transform_one_word(w: str) -> str:
        if w == "":
            return w
        last = w[-1]
        res = ""
        for ch in w[:-1]:
            if ch != last:
                res += ch
        res += last
        return res

    def run():
        s = inp.get("1.0", "end-1c")
        if mode.get() == "no":
            r = s
        else:
            r = ""
            cur = ""
            for ch in s:
                if ch.isspace():
                    if cur != "":
                        r += transform_one_word(cur)
                        cur = ""
                    r += ch  # пробелы оставляем как есть
                else:
                    cur += ch
            if cur != "":
                r += transform_one_word(cur)

        out.delete("1.0", "end")
        out.insert("1.0", r)

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=6)
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()

