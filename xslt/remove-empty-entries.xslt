<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output encoding="UTF-8" method="xml" indent="yes" />
    <xsl:template match="/">
        <xsl:apply-templates select="root"/>
    </xsl:template>
    <xsl:template match="root">
        <root>
            <xsl:apply-templates select="resheader"/>
            <xsl:apply-templates select="data"/>
        </root>
    </xsl:template>
    <xsl:template match="resheader">
        <xsl:copy-of select="."/>
    </xsl:template>
    <xsl:template match="data">
        <xsl:if test="value != ''">
            <xsl:copy-of select="."/>
        </xsl:if>
    </xsl:template>
</xsl:stylesheet>
