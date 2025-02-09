/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["./**/*.{razor,html}"],
    theme: {
        extend: {},
    },
    plugins: [require('daisyui')],
    daisyui: {
        themes: [

            {
                light: {
                    ...require("daisyui/src/theming/themes")["light"],
                    "primary": "facc15",
                    "secondary": "003366",

                },

                dark: {
                    ...require("daisyui/src/theming/themes")["dark"],
                    "primary": "facc15",
                    "primary-content": "111111",
                    "secondary": "ffffff"

                },





            },


        ], // false: only light + dark | true: all themes | array: specific themes like this ["light", "dark", "cupcake"]
        darkTheme: "false", // name of one of the included themes for dark mode
        base: true, // applies background color and foreground color for root element by default
        styled: true, // include daisyUI colors and design decisions for all components
        utils: true, // adds responsive and modifier utility classes
        prefix: "", // prefix for daisyUI classnames (components, modifiers and responsive class names. Not colors)
        logs: true, // Shows info about daisyUI version and used config in the console when building your CSS
        themeRoot: ":root", // The element that receives theme color CSS variables
    }

}

